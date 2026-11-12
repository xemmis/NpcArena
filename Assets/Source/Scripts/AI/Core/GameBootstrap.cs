namespace AI.Core
{
    using AI.Interface;
    using UnityEngine;
    using UnityEngine.AI;

    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _cellSize = 10;

        public INpcRegistry Registry { get; private set; }
        public INpcFactory Factory { get; private set; }

        private void Awake()
        {
            Registry = new NpcRegistry(_cellSize);
            var searcher = new NpcSearcher();
            searcher.Initialize(Registry, _cellSize);

            NpcLocator.CellSize = _cellSize;
            NpcLocator.Initialize(Registry, searcher);

            Factory = new NpcFactory(new NpcPool());
        }
        private void Start()
        {
            NavMesh.avoidancePredictionTime = 0.5f;
        }

        private void OnDestroy()
        {
            NpcLocator.Reset();
        }
    }
}
