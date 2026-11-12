namespace AI.Core
{
    using AI.Interface;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class NpcPool : INpcPool
    {
        private readonly Transform _root;
        private readonly Dictionary<GameObject, Stack<GameObject>> _free = new();
        private readonly Dictionary<GameObject, GameObject> _prefabOf = new();

        public NpcPool()
        {
            var root = new GameObject("NpcPool");
            root.SetActive(false);
            _root = root.transform;
        }

        public void Prewarm(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++)
                Push(prefab, Create(prefab));
        }

        public GameObject Get(GameObject prefab)
        {
            if (_free.TryGetValue(prefab, out var stack))
            {
                while (stack.TryPop(out var npc))
                    if (npc != null) return npc;
            }

            return Create(prefab);
        }

        public void Release(GameObject npc)
        {
            if (!_prefabOf.TryGetValue(npc, out var prefab))
            {
                Object.Destroy(npc);
                return;
            }

            npc.transform.SetParent(_root);
            Push(prefab, npc);
        }

        private GameObject Create(GameObject prefab)
        {
            var npc = Object.Instantiate(prefab, _root);
            _prefabOf[npc] = prefab;
            return npc;
        }

        private void Push(GameObject prefab, GameObject npc)
        {
            if (!_free.TryGetValue(prefab, out var stack))
                _free[prefab] = stack = new Stack<GameObject>();
            stack.Push(npc);
        }
    }
}
