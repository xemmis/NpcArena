using UnityEngine;

namespace AI.Interface
{
    public interface INpcPool
    {
        GameObject Get(GameObject prefab);
        void Release(GameObject npc);
    }
}
