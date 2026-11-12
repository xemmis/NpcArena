namespace AI.Testing
{
    using AI.Core;
    using AI.Interface;
    using System;
    using System.Text;
    using UnityEngine;

    // HUD для замеров: FPS, время кадра, сколько живых в каждой фракции.
    // Текст обновляю раз в _refreshInterval, иначе HUD сам мусорит в профайлере.
    public sealed class StatsHud : MonoBehaviour
    {
        private static readonly Faction[] Factions = (Faction[])Enum.GetValues(typeof(Faction));

        [SerializeField] private GameBootstrap _bootstrap = null;
        [SerializeField, Min(0.1f)] private float _refreshInterval = 0.5f;
        [SerializeField, Min(8)] private int _fontSize = 22;
        [SerializeField] private Vector2 _offset = new(12f, 12f);

        private readonly StringBuilder _builder = new(256);

        private INpcRegistry _registry;
        private GUIStyle _style;
        private GUIStyle _shadowStyle;
        private string _text = string.Empty;

        private float _elapsed;
        private int _frames;
        private float _worstFrame;

        private void Start()
        {
            if (_bootstrap == null) _bootstrap = FindFirstObjectByType<GameBootstrap>();
            if (_bootstrap != null) _registry = _bootstrap.Registry;

            if (_registry == null)
                Debug.LogWarning($"{nameof(StatsHud)}: {nameof(GameBootstrap)} or its registry not found, unit counts are hidden", this);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            _elapsed += dt;
            _frames++;
            if (dt > _worstFrame) _worstFrame = dt;

            if (_elapsed < _refreshInterval) return;

            Rebuild();

            _elapsed = 0f;
            _frames = 0;
            _worstFrame = 0f;
        }

        private void Rebuild()
        {
            float avgMs = _elapsed / _frames * 1000f;
            float fps = _frames / _elapsed;

            _builder.Clear();
            _builder.Append("FPS: ").Append(Mathf.RoundToInt(fps))
                    .Append("   avg ").Append(avgMs.ToString("0.0")).Append(" ms")
                    .Append("   worst ").Append((_worstFrame * 1000f).ToString("0.0")).Append(" ms");

            if (_registry != null)
            {
                int total = 0;
                _builder.Append('\n');

                foreach (var faction in Factions)
                {
                    int count = _registry.CountOf(faction);
                    total += count;
                    _builder.Append(faction).Append(": ").Append(count).Append("   ");
                }

                _builder.Append("\nAlive: ").Append(total);
            }

            _text = _builder.ToString();
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = _fontSize, fontStyle = FontStyle.Bold };
                _style.normal.textColor = Color.white;
                _shadowStyle = new GUIStyle(_style);
                _shadowStyle.normal.textColor = Color.black;
            }

            var rect = new Rect(_offset.x, _offset.y, Screen.width, Screen.height);
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), _text, _shadowStyle);
            GUI.Label(rect, _text, _style);
        }
    }
}
