namespace AI.Core
{
    using AI.Interface;
    using UnityEngine;

    public sealed class NpcFactory : INpcFactory
    {
        private readonly INpcPool _pool;

        public NpcFactory(INpcPool pool)
        {
            _pool = pool;
        }

        public GameObject Spawn(NpcData data, Vector3 position, Faction faction)
        {
            var npc = _pool.Get(data.NpcPrefab);
            npc.GetComponent<NpcBehaviorLogic>().Initialize(data, faction, this);
            npc.transform.SetPositionAndRotation(position, Quaternion.identity);
            npc.transform.SetParent(null);
            return npc;
        }

        public void Despawn(GameObject npc)
        {
            _pool.Release(npc);
        }
    }
}
