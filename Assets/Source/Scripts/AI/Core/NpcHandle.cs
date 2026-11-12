namespace AI.Core
{
    using System;
    using UnityEngine;

    public readonly struct NpcHandle : IEquatable<NpcHandle>
    {
        public readonly int Id;
        public readonly int Generation;
        public readonly NpcBehaviorLogic Co;
        public readonly Faction Faction;

        public NpcHandle(int id, int generation, NpcBehaviorLogic co, Faction faction)
        {
            Id = id;
            Generation = generation;
            Co = co;
            Faction = faction;
        }

        public bool IsValid => Co != null && Co.Generation == Generation && Co.Data.IsAlive;
        public Vector3 Position => Co.transform.position;

        public bool Equals(NpcHandle other) => Id == other.Id && Generation == other.Generation;
        public override bool Equals(object obj) => obj is NpcHandle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id, Generation);
    }
}
