namespace AI.States
{
    using AI.Core;
    using AI.Interface;
    using UnityEngine;
    using UnityEngine.AI;

    // Движение в зону сбора, пока рядом нет врагов.
    // Бой приоритетнее: каждые ~0.5 s состояние зовёт ActionDone -> Think(),
    // и если враг найден, NPC сразу переходит в Attack / Chase / Retreat / Defend.
    public class Advance : IState
    {
        private const float ThinkMin = .45f;
        private const float ThinkMax = .65f;
        private const float SampleRadius = 4f;
        private const float StoppingDistance = 0.5f;

        private readonly RallyArea _area;
        private NavMeshAgent _agent = null;
        private float _thinkTimer = 0;

        public Advance(RallyArea area)
        {
            _area = area;
        }

        public void Enter(INpcController Self)
        {
            _agent = Self.GetAgent();
            _agent.stoppingDistance = StoppingDistance;
            _agent.isStopped = false;
            _thinkTimer = Random.Range(ThinkMin, ThinkMax);

            PickDestination();
        }

        public void Exit(INpcController Self)
        {
            _agent = null;
        }

        public void Update(INpcController Self)
        {
            if (_area.Roam && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                PickDestination();

            _thinkTimer -= Time.deltaTime;
            if (_thinkTimer > 0) return;

            _thinkTimer = Random.Range(ThinkMin, ThinkMax);
            Self.ActionDone();
        }

        // Каждый NPC идёт не в центр, а в случайную точку круга,
        // иначе тысячи агентов сбиваются в одну кучу и avoidance взрывается.
        private void PickDestination()
        {
            var offset = Random.insideUnitCircle * _area.Radius;
            var destination = _area.Center + new Vector3(offset.x, 0f, offset.y);

            if (NavMesh.SamplePosition(destination, out var hit, SampleRadius, NavMesh.AllAreas))
                destination = hit.position;

            _agent.SetDestination(destination);
        }
    }
}
