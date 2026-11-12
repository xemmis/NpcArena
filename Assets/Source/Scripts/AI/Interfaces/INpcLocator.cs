using AI.Core;
using UnityEngine;

namespace AI.Interface
{
    public interface INpcLocator
    {
        bool TryFindNearestEnemy(Vector3 position, float radius, Faction myFaction, out NpcHandle enemy);
    }
}
