namespace AI.States
{
    using AI.Core;
    using AI.Interface;
    using UnityEngine;
    using UnityEngine.AI;

    public class Chase : IState
    {
        public Chase(NpcHandle target)
        {
            _target = target;
        }

        private NpcHandle _target;
        private NavMeshAgent _agent = null;
        private float _thinkTimer = 0;
        private float _repathTimer = 0;
        private Vector3 _lastTargetPosition;

        private const float ThinkInterval = 1f;

        private const float RepathInterval = 0.25f;
        private const float RepathDistance = 0.5f;
        private const float RepathSqrDistance = RepathDistance * RepathDistance;

        public void Enter(INpcController Self)
        {
            _agent = Self.GetAgent();
            _agent.stoppingDistance = Self.Data.AttackRange;
            _agent.isStopped = false;
            _thinkTimer = ThinkInterval;
            _repathTimer = RepathInterval;

            if (_target.IsValid) Repath();
        }

        public void Exit(INpcController Self)
        {
            _target = default;
            _agent = null;
        }

        public void Update(INpcController Self)
        {
            if (!_target.IsValid)
            {
                Self.ActionDone();
                return;
            }

            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0)
            {
                _repathTimer = RepathInterval;

                if ((_target.Position - _lastTargetPosition).sqrMagnitude >= RepathSqrDistance)
                    Repath();
            }

            _thinkTimer -= Time.deltaTime;

            if (_thinkTimer <= 0 || (!_agent.pathPending && _agent.remainingDistance <= Self.Data.AttackRange))
            {
                Self.ActionDone();
            }
        }

        private void Repath()
        {
            _lastTargetPosition = _target.Position;
            _agent.SetDestination(_lastTargetPosition);
        }
    }
}
