namespace AI.Testing
{
    using AI.Core;
    using AI.Interface;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;

    public sealed class ArenaSpawner : MonoBehaviour
    {
        [System.Serializable]
        private struct Squad
        {
            public Faction Faction;
            public NpcData Data;
            public Color GizmoColor;
        }

        // Squads - каждая фракция отдельным отрядом по кругу, отряды сходятся в центре.
        // Mixed  - все юниты одной сеткой вокруг центра, фракции чередуются шахматкой:
        //          у каждого юнита враг стоит вплотную с первого кадра.
        // Roam   - все юниты одной фракции (первый отряд) одной сеткой, без боя бродят
        //          по случайным точкам вокруг центра. Замер стоимости движения толпы.
        private enum SpawnLayout { Squads, Mixed, Roam }

        [SerializeField] private GameBootstrap _bootstrap = null;
        [SerializeField] private SpawnLayout _layout = SpawnLayout.Squads;
        [SerializeField] private Squad[] _squads =
        {
            new Squad { Faction = Faction.Player, GizmoColor = Color.blue },
            new Squad { Faction = Faction.Enemy, GizmoColor = Color.red },
            new Squad { Faction = Faction.Neutral, GizmoColor = Color.green },
        };

        [SerializeField, Min(1)] private int _unitsPerSquad = 100;
        [SerializeField, Min(0.5f)] private float _spacing = 1.5f;
        [SerializeField, Min(1f)] private float _engageDistance = 8f;
        [SerializeField, Min(1)] private int _spawnPerFrame = 25;
        [SerializeField, Min(0.1f)] private float _navMeshSampleRadius = 2f;
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField] private bool _advanceToCenter = true;
        [SerializeField, Min(1f)] private float _rallyRadius = 6f;
        [SerializeField, Min(1f)] private float _roamRadius = 60f;

        private readonly List<GameObject> _spawned = new();
        private Coroutine _spawnRoutine = null;

        private void Start()
        {
            if (_layout == SpawnLayout.Roam)
                NpcLocator.SetRallyArea(new RallyArea(transform.position, _roamRadius, roam: true));
            else if (_advanceToCenter)
                NpcLocator.SetRallyArea(new RallyArea(transform.position, _rallyRadius, roam: false));

            if (_spawnOnStart) Spawn();
        }

        private void OnDisable()
        {
            NpcLocator.ClearRallyArea();
        }

        [ContextMenu("Spawn")]
        public void Spawn()
        {
            if (!Application.isPlaying) return;
            if (!TryGetFactory(out var factory) || !Validate()) return;

            Clear();
            _spawnRoutine = StartCoroutine(_layout == SpawnLayout.Squads
                ? SpawnRoutine(factory)
                : GridSpawnRoutine(factory, mixFactions: _layout == SpawnLayout.Mixed));
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }

            if (!TryGetFactory(out var factory)) return;

            foreach (var npc in _spawned)
                if (npc != null && npc.activeInHierarchy)
                    factory.Despawn(npc);

            _spawned.Clear();
        }

        private IEnumerator SpawnRoutine(INpcFactory factory)
        {
            int spawnedThisFrame = 0;
            int skipped = 0;

            for (int i = 0; i < _squads.Length; i++)
            {
                var squad = _squads[i];
                GetSquadFrame(i, out var center, out var forward, out var right);

                for (int u = 0; u < _unitsPerSquad; u++)
                {
                    var position = GetUnitPosition(u, center, forward, right);

                    if (!NavMesh.SamplePosition(position, out var hit, _navMeshSampleRadius, NavMesh.AllAreas))
                    {
                        skipped++;
                        continue;
                    }

                    _spawned.Add(factory.Spawn(squad.Data, hit.position, squad.Faction));

                    if (++spawnedThisFrame < _spawnPerFrame) continue;

                    spawnedThisFrame = 0;
                    yield return null;
                }
            }

            if (skipped > 0)
                Debug.LogWarning($"{nameof(ArenaSpawner)}: {skipped} units skipped, no NavMesh within {_navMeshSampleRadius}m", this);

            _spawnRoutine = null;
        }

        private IEnumerator GridSpawnRoutine(INpcFactory factory, bool mixFactions)
        {
            int total = _unitsPerSquad * _squads.Length;
            int columns = Mathf.CeilToInt(Mathf.Sqrt(total));
            float half = (columns - 1) * _spacing * 0.5f;

            int spawnedThisFrame = 0;
            int skipped = 0;

            for (int i = 0; i < total; i++)
            {
                int column = i % columns;
                int row = i / columns;

                // (column + row) вместо i: при i % n фракции легли бы полосами, если columns кратно n.
                // Так соседи и по строке, и по столбцу всегда из чужих фракций.
                var squad = mixFactions ? _squads[(column + row) % _squads.Length] : _squads[0];

                var position = transform.position + new Vector3(column * _spacing - half, 0f, row * _spacing - half);

                if (!NavMesh.SamplePosition(position, out var hit, _navMeshSampleRadius, NavMesh.AllAreas))
                {
                    skipped++;
                    continue;
                }

                _spawned.Add(factory.Spawn(squad.Data, hit.position, squad.Faction));

                if (++spawnedThisFrame < _spawnPerFrame) continue;

                spawnedThisFrame = 0;
                yield return null;
            }

            if (skipped > 0)
                Debug.LogWarning($"{nameof(ArenaSpawner)}: {skipped} units skipped, no NavMesh within {_navMeshSampleRadius}m", this);

            _spawnRoutine = null;
        }

        private bool TryGetFactory(out INpcFactory factory)
        {
            if (_bootstrap == null) _bootstrap = FindFirstObjectByType<GameBootstrap>();

            factory = _bootstrap != null ? _bootstrap.Factory : null;
            if (factory != null) return true;

            Debug.LogError($"{nameof(ArenaSpawner)}: {nameof(GameBootstrap)} or its factory not found", this);
            return false;
        }

        private bool Validate()
        {
            for (int i = 0; i < _squads.Length; i++)
            {
                if (_squads[i].Data != null && _squads[i].Data.NpcPrefab != null) continue;

                Debug.LogError($"{nameof(ArenaSpawner)}: squad {i} has no {nameof(NpcData)} or prefab", this);
                return false;
            }

            return _squads.Length > 0;
        }

        private (int columns, int rows) GetGridSize()
        {
            int columns = Mathf.CeilToInt(Mathf.Sqrt(_unitsPerSquad));
            int rows = Mathf.CeilToInt((float)_unitsPerSquad / columns);
            return (columns, rows);
        }

        private Vector2 GetHalfExtents()
        {
            var (columns, rows) = GetGridSize();
            return new Vector2((columns - 1) * _spacing * 0.5f, (rows - 1) * _spacing * 0.5f);
        }

        private float GetInnerEdgeDistance()
        {
            if (_squads.Length < 2) return 0f;

            float halfAngle = Mathf.PI / _squads.Length;
            float halfWidth = GetHalfExtents().x;

            float engage = _engageDistance / (2f * Mathf.Sin(halfAngle));
            float noOverlap = halfWidth / Mathf.Tan(halfAngle) + _spacing;

            return Mathf.Max(engage, noOverlap);
        }

        private void GetSquadFrame(int index, out Vector3 center, out Vector3 forward, out Vector3 right)
        {
            float angle = index * Mathf.PI * 2f / _squads.Length;
            var outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            forward = -outward;
            right = Vector3.Cross(Vector3.up, forward);
            center = transform.position + outward * (GetInnerEdgeDistance() + GetHalfExtents().y);
        }

        private Vector3 GetUnitPosition(int index, Vector3 squadCenter, Vector3 forward, Vector3 right)
        {
            var (columns, _) = GetGridSize();
            var half = GetHalfExtents();

            int column = index % columns;
            int row = index / columns;

            return squadCenter
                + right * (column * _spacing - half.x)
                + forward * (half.y - row * _spacing);
        }

        private void OnDrawGizmosSelected()
        {
            if (_squads == null || _squads.Length == 0) return;

            if (_layout != SpawnLayout.Squads)
            {
                int columns = Mathf.CeilToInt(Mathf.Sqrt(_unitsPerSquad * _squads.Length));
                float side = columns * _spacing;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, new Vector3(side, 0.1f, side));

                if (_layout == SpawnLayout.Roam)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(transform.position, _roamRadius);
                }
                return;
            }

            var half = GetHalfExtents();
            var size = new Vector3(half.x * 2f + _spacing, 0.1f, half.y * 2f + _spacing);

            for (int i = 0; i < _squads.Length; i++)
            {
                GetSquadFrame(i, out var center, out var forward, out _);

                Gizmos.color = _squads[i].GizmoColor;
                Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(forward), Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, size);
            }

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, GetInnerEdgeDistance());
        }
    }
}
