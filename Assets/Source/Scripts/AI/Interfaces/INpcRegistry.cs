using AI.Core;
using System.Collections.Generic;

namespace AI.Interface
{
    public interface INpcRegistry
    {
        IReadOnlyCollection<NpcHandle> All { get; }

        void Register(NpcHandle npc);
        void Unregister(NpcHandle npc);
        bool TryGetIdsAt(int x, int z, Faction faction, out HashSet<int> ids);
        int CountOf(Faction faction);
        NpcHandle GetById(int id);
        void UpdateCell(NpcHandle npc);
    }
}
