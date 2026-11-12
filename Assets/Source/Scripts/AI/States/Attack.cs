namespace AI.States
{
    using AI.Core;
    using AI.Interface;
    using UnityEngine;
    using UnityEngine.AI;

    public class Attack : IState
    {
        public Attack(NpcHandle target)
        {
            _target = target;
        }

        private NpcHandle _target;
        private Animator _animator = null;
        private float _timer = 0;
        private int _damage = 0;
        private NavMeshAgent _agent = null;
        private const float ReachTolerance = 1.5f;

        public void Enter(INpcController Self)
        {
            _animator = Self.GetAnimator();
            _damage = Self.Data.Damage;
            _timer = Self.Data.HitDelay;
            _agent = Self.GetAgent();
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            Self.GetAgent().ResetPath();
            _animator.SetTrigger("Attack");
        }

        public void Exit(INpcController Self)
        {
            _target = default;
            _animator = null;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            _agent = null;
        }

        public void Update(INpcController Self)
        {
            if (!_target.IsValid)
            {
                Self.ActionDone();
                return;
            }

            _timer -= Time.deltaTime;

            if (_timer > 0) return;

            var distance = Vector3.Distance(Self.GetAgent().transform.position, _target.Position);
            if (distance <= Self.Data.AttackRange * ReachTolerance)
                _target.Co.ApplyDamage(_damage);

            Self.ActionDone();
        }
    }
}
