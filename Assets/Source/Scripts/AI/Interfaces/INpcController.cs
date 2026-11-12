namespace AI.Interface
{
    using AI.Core;
    using UnityEngine;
    using UnityEngine.AI;

    public interface INpcController
    {
        Npc Data { get; }
        void ChangeState(IState newstate);
        void ActionDone();
        void Despawn();
        void SetDefending(bool value);
        bool TryFindEnemy(out NpcHandle enemy);

        void ApplyDamage(int amount);
        void ApplyHeal(int amount);

        Animator GetAnimator();
        NavMeshAgent GetAgent();
    }
}
