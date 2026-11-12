using AI.Core;
using UnityEngine;

namespace AI.Interface
{
    public interface INpcFactory
    {
        GameObject Spawn(NpcData data, Vector3 position, Faction faction);
        void Despawn(GameObject npc);
    }
}
