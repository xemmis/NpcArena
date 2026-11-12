namespace AI.Core
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "NpcData", menuName = "AI/Npc Data")]
    public class NpcData : ScriptableObject
    {
        [field: SerializeField] public int Health { get; private set; }
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public int AttackRange { get; private set; }
        [field: SerializeField] public int RetreatRange { get; private set; }
        [field: SerializeField] public float HitDelay { get; private set; }

        [field: SerializeField] public GameObject NpcPrefab { get; private set; }
    }
}
