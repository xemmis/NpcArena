namespace AI.States
{
    using AI.Interface;
    using UnityEngine;
    using UnityEngine.AI;

    public class Defend : IState
    {
        private float _timer = 0;
        private NavMeshAgent _agent = null;

        public void Enter(INpcController Self)
        {
            _timer = Random.Range(1f, 3f);

            Self.GetAgent().ResetPath();
            Self.GetAnimator().SetBool("Defend", true);
            Self.SetDefending(true);
            _agent = Self.GetAgent();
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        public void Exit(INpcController Self)
        {
            Self.SetDefending(false);
            Self.GetAnimator().SetBool("Defend", false);
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            _agent = null;
        }

        public void Update(INpcController Self)
        {
            _timer -= Time.deltaTime;

            if (_timer > 0) return;

            Self.ActionDone();
        }
    }
}
