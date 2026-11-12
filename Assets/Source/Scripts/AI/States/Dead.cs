namespace AI.States
{
    using AI.Interface;
    using UnityEngine;

    public class Dead : IState
    {
        private const float DespawnDelay = 3f;

        private float _timer = 0;

        public void Enter(INpcController Self)
        {
            _timer = DespawnDelay;

            Self.GetAgent().ResetPath();
            Self.GetAnimator().SetTrigger("Dead");
        }

        public void Exit(INpcController Self)
        {

        }

        public void Update(INpcController Self)
        {
            _timer -= Time.deltaTime;

            if (_timer > 0) return;

            Self.Despawn();
        }
    }
}
