namespace AI.Core
{
    using UnityEngine;

    [System.Serializable]
    public struct Npc
    {
        public Npc(int hp, int dmg, int atkRange, int retreatRange, float delay)
        {
            Health = hp;
            Damage = dmg;
            AttackRange = atkRange;
            RetreatRange = retreatRange;
            HitDelay = delay;
            IsAlive = true;
        }

        [field: SerializeField] public int Health { get; private set; }
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public int AttackRange { get; private set; }
        [field: SerializeField] public int RetreatRange { get; private set; }
        [field: SerializeField] public float HitDelay { get; private set; }
        [field: SerializeField] public bool IsAlive { get; private set; }

        public void TakeHit(int Damage)
        {
            if (Damage <= 0) return;

            Health -= Damage;

            if (Health <= 0) IsAlive = false;
        }

        public void TakeHeal(int Heal)
        {
            if (Heal <= 0 || !IsAlive) return;

            Health += Heal;
        }
    }
}
