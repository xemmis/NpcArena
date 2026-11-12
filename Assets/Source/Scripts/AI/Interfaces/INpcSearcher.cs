using AI.Core;
using UnityEngine;

namespace AI.Interface
{
    public interface INpcSearcher
    {
        bool TryFindNearestEnemy(Vector3 position, float radius, Faction myFaction, out NpcHandle enemy);
    }
}
