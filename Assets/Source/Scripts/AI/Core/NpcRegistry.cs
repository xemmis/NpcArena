namespace AI.Core
{
    using AI.Interface;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class NpcRegistry : INpcRegistry
    {
        private readonly int _cellSize;
        private readonly Dictionary<int, NpcHandle> _byId = new();
        private readonly Dictionary<int, (int x, int z, Faction faction)> _cellOf = new();
        private readonly Dictionary<(int x, int z, Faction faction), HashSet<int>> _byCell = new();
        private readonly List<NpcHandle> _all = new();
        private readonly int[] _countByFaction = new int[Enum.GetValues(typeof(Faction)).Length];

        public NpcRegistry(int cellSize)
        {
            _cellSize = cellSize;
        }

        public IReadOnlyCollection<NpcHandle> All => _all;

        public void Register(NpcHandle npc)
        {
            if (_byId.ContainsKey(npc.Id)) return;

            var key = CellOf(npc.Position, npc.Faction);
            _byId[npc.Id] = npc;
            _cellOf[npc.Id] = key;
            _all.Add(npc);
            _countByFaction[(int)npc.Faction]++;
            AddToCell(npc.Id, key);
        }

        public void Unregister(NpcHandle npc)
        {
            if (!_byId.TryGetValue(npc.Id, out var stored) || !stored.Equals(npc)) return;

            _byId.Remove(npc.Id);
            _all.Remove(npc);
            _countByFaction[(int)npc.Faction]--;

            if (_cellOf.Remove(npc.Id, out var key))
                RemoveFromCell(npc.Id, key);
        }

        public bool TryGetIdsAt(int x, int z, Faction faction, out HashSet<int> ids)
            => _byCell.TryGetValue((x, z, faction), out ids);

        public int CountOf(Faction faction)
            => _countByFaction[(int)faction];

        public NpcHandle GetById(int id)
            => _byId.TryGetValue(id, out var npc) ? npc : default;

        public void UpdateCell(NpcHandle npc)
        {
            if (!_cellOf.TryGetValue(npc.Id, out var oldKey)) return;

            var newKey = CellOf(npc.Position, npc.Faction);
            if (newKey == oldKey) return;

            RemoveFromCell(npc.Id, oldKey);
            AddToCell(npc.Id, newKey);
            _cellOf[npc.Id] = newKey;
        }

        private void AddToCell(int id, (int x, int z, Faction faction) key)
        {
            if (!_byCell.TryGetValue(key, out var set))
                _byCell[key] = set = new HashSet<int>();
            set.Add(id);
        }

        private void RemoveFromCell(int id, (int x, int z, Faction faction) key)
        {
            if (!_byCell.TryGetValue(key, out var set)) return;

            set.Remove(id);
            if (set.Count == 0) _byCell.Remove(key);
        }

        private (int x, int z, Faction faction) CellOf(Vector3 pos, Faction faction)
            => (Mathf.FloorToInt(pos.x / _cellSize), Mathf.FloorToInt(pos.z / _cellSize), faction);
    }
}
