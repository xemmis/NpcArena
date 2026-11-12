namespace AI.States
{
    using AI.Interface;
    using UnityEngine;

    public class Idle : IState
    {
        private Animator _animator = null;
        private float _idleThinkTick = Random.Range(.450f, .650f);

        public void Enter(INpcController Self)
        {
            _animator = Self.GetAnimator();
            _animator.enabled = false;
        }

        public void Exit(INpcController Self)
        {
            _animator.enabled = true;
            _animator = null;
        }

        public void Update(INpcController Self)
        {
            _idleThinkTick -= Time.deltaTime;
            if (_idleThinkTick <= 0)
            {
                Self.ActionDone();
                _idleThinkTick = Random.Range(.450f, .650f);
            }
        }
    }
}
