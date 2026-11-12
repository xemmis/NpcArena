namespace AI.Core
{
    using AI.Interface;
    using UnityEngine;

    public static class NpcLocator
    {
        private static INpcRegistry _registry;
        private static INpcSearcher _searcher;
        private static RallyArea? _rallyArea;

        public static int CellSize { get; set; } = 1;

        public static void Initialize(INpcRegistry registry, INpcSearcher searcher)
        {
            _registry = registry;
            _searcher = searcher;
        }

        public static void Reset()
        {
            _registry = null;
            _searcher = null;
            _rallyArea = null;
            CellSize = 1;
        }

        // Зона сбора: NPC без врагов в радиусе идут в неё (состояние Advance).
        // Если зона не задана, NPC стоят в Idle, как раньше.
        public static void SetRallyArea(RallyArea area) => _rallyArea = area;
        public static void ClearRallyArea() => _rallyArea = null;

        public static bool TryGetRallyArea(out RallyArea area)
        {
            area = _rallyArea.GetValueOrDefault();
            return _rallyArea.HasValue;
        }

        public static void Register(NpcHandle npc)
        {
            _registry?.Register(npc);
        }

        public static void Unregister(NpcHandle npc)
        {
            _registry?.Unregister(npc);
        }

        public static void UpdateCell(NpcHandle npc)
        {
            _registry?.UpdateCell(npc);
        }

        public static bool TryFindNearestEnemy(Vector3 position, float radius, Faction myFaction, out NpcHandle enemy)
        {
            if (_searcher == null)
            {
                Debug.LogError("NpcLocator is not initialized");
                enemy = default;
                return false;
            }

            return _searcher.TryFindNearestEnemy(position, radius, myFaction, out enemy);
        }
    }
}
