namespace AI.Core
{
    using AI.Interface;
    using UnityEngine;

    public static class NpcLocator
    {
        private static INpcRegistry _registry;
        private static INpcSearcher _searcher;

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
            CellSize = 1;
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
