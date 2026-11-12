namespace AI.Core
{
    using AI.Interface;
    using System;
    using UnityEngine;

    public sealed class NpcSearcher : INpcSearcher
    {
        // Кэшируем один раз: Enum.GetValues аллоцирует массив при каждом вызове.
        private static readonly Faction[] Factions = (Faction[])Enum.GetValues(typeof(Faction));

        private INpcRegistry _registry;
        private int _cellSize;

        public void Initialize(INpcRegistry registry, int cellSize)
        {
            _registry = registry;
            _cellSize = cellSize;
        }

        public bool TryFindNearestEnemy(Vector3 position, float radius, Faction myFaction, out NpcHandle enemy)
        {
            var center = CellOf(position);
            int cellRadius = Mathf.CeilToInt(radius / _cellSize);
            float sqrRadius = radius * radius;

            enemy = default;
            float bestSqr = float.MaxValue;

            for (int x = -cellRadius; x <= cellRadius; x++)
            {
                for (int z = -cellRadius; z <= cellRadius; z++)
                {
                    foreach (var faction in Factions)
                    {
                        if (faction == myFaction) continue;
                        if (!_registry.TryGetIdsAt(center.x + x, center.z + z, faction, out var ids)) continue;

                        foreach (int id in ids)
                        {
                            var npc = _registry.GetById(id);
                            if (!npc.IsValid) continue;

                            float sqr = (npc.Position - position).sqrMagnitude;
                            if (sqr > sqrRadius || sqr >= bestSqr) continue;

                            bestSqr = sqr;
                            enemy = npc;
                        }
                    }
                }
            }

            return enemy.Co != null;
        }

        private (int x, int z) CellOf(Vector3 pos)
            => (Mathf.FloorToInt(pos.x / _cellSize), Mathf.FloorToInt(pos.z / _cellSize));
    }
}
