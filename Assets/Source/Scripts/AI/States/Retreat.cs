namespace AI.States
{
    using AI.Core;
    using AI.Interface;
    using UnityEngine;
    using UnityEngine.AI;

    public class Retreat : IState
    {
        public Retreat(NpcHandle target)
        {
            _target = target;
        }

        private NpcHandle _target;
        private NavMeshAgent _agent = null;
        private float _retreatTimer = 0;

        public void Enter(INpcController Self)
        {
            _agent = Self.GetAgent();
            _agent.stoppingDistance = 0;
            _agent.isStopped = false;

            _retreatTimer = UnityEngine.Random.Range(3, 12);

            if (!_target.IsValid)
            {
                _retreatTimer = 0;
                return;
            }

            var from = _agent.transform.position;
            var away = (from - _target.Position).normalized;
            var target = from + away * Self.Data.RetreatRange;

            if (NavMesh.SamplePosition(target, out var hit, Self.Data.RetreatRange, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
            else
                _retreatTimer = 0;
        }

        public void Exit(INpcController Self)
        {
            _target = default;
            _agent = null;
        }

        public void Update(INpcController Self)
        {
            _retreatTimer -= Time.deltaTime;

            if (_retreatTimer <= 0 || (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance))
            {
                Self.ActionDone();
            }
        }
    }
}
