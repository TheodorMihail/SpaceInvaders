using System.Collections.Generic;
using System.Linq;
using SpaceInvaders.Scenes.Game;
using UnityEditor;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>
    /// Editor window that draws every authored wave formation, for checking level pacing and spread
    /// without entering play mode.
    /// </summary>
    public class LevelWavesVisualizerWindow : EditorWindow
    {
        private static readonly Color BackgroundColor = new Color(0.13f, 0.13f, 0.18f);
        private static readonly Color ArrivalLineColor = new Color(0.35f, 0.75f, 0.45f, 0.7f);
        private static readonly Color ScreenEdgeColor = new Color(0.9f, 0.9f, 0.4f, 0.35f);
        private static readonly Color OffScreenColor = new Color(1f, 0.3f, 0.3f);

        private const float WaveHeight = 150f;
        private const float SlotSize = 9f;
        private const float BoundsMargin = 40f;

        [SerializeField] private float _orthographicSize = 150f;
        [SerializeField] private float _aspectRatio = 16f / 9f;
        [SerializeField] private int _levelFilter;

        private Vector2 _scrollPosition;
        private List<LevelConfigSO> _levels = new List<LevelConfigSO>();

        private float ScreenHalfWidth => _orthographicSize * _aspectRatio;

        private void OnEnable()
        {
            ReloadLevels();
        }

        [MenuItem("SpaceInvaders/Level Waves Visualizer")]
        private static void ShowWindow()
        {
            LevelWavesVisualizerWindow window = GetWindow<LevelWavesVisualizerWindow>("Level Waves");
            window.minSize = new Vector2(520f, 480f);
            window.Show();
        }

        private void OnFocus()
        {
            ReloadLevels();
        }

        private void ReloadLevels()
        {
            _levels = AssetDatabase.FindAssets($"t:{nameof(LevelConfigSO)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<LevelConfigSO>)
                .Where(level => level != null)
                .OrderBy(level => level.Index)
                .ToList();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_levels.Count == 0)
            {
                EditorGUILayout.HelpBox("No LevelConfigSO assets found.", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _levels.Count; i++)
            {
                if (_levelFilter == 0 || _levelFilter - 1 == i)
                {
                    DrawLevel(_levels[i]);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                string[] options = new string[_levels.Count + 1];
                options[0] = "All levels";
                for (int i = 0; i < _levels.Count; i++)
                {
                    options[i + 1] = $"Level {_levels[i].Index}";
                }

                _levelFilter = EditorGUILayout.Popup(_levelFilter, options, EditorStyles.toolbarPopup, GUILayout.Width(120f));

                GUILayout.Space(10f);
                EditorGUILayout.LabelField("Ortho Size", GUILayout.Width(65f));
                _orthographicSize = EditorGUILayout.FloatField(_orthographicSize, EditorStyles.toolbarTextField, GUILayout.Width(45f));
                EditorGUILayout.LabelField("Aspect", GUILayout.Width(45f));
                _aspectRatio = EditorGUILayout.FloatField(_aspectRatio, EditorStyles.toolbarTextField, GUILayout.Width(45f));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    ReloadLevels();
                }
            }
        }

        private void DrawLevel(LevelConfigSO level)
        {
            List<WaveConfigDTO> waves = level.WavesConfigs;
            int total = waves.Sum(wave => wave.WavesFormation.Count);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    $"Level {level.Index}  ({level.LevelType})",
                    EditorStyles.boldLabel, GUILayout.Width(160f));
                EditorGUILayout.LabelField(
                    $"{waves.Count} waves | {total} enemies | 3star <= {level.ThreeStarMaxDamage} damage");

                if (GUILayout.Button("Select", GUILayout.Width(60f)))
                {
                    Selection.activeObject = level;
                    EditorGUIUtility.PingObject(level);
                }
            }

            for (int i = 0; i < waves.Count; i++)
            {
                DrawWave(waves[i], i + 1);
            }
        }

        private void DrawWave(WaveConfigDTO wave, int waveNumber)
        {
            List<WaveConfigDTO.WaveFormationDTO> slots = wave.WavesFormation;

            EditorGUILayout.LabelField(
                $"   Wave {waveNumber}   {DescribeMix(slots)}   entry speed {wave.EntrySpeed:0}   gap {wave.TimeBetweenSpawns:0.##}s",
                EditorStyles.miniLabel);

            Rect area = GUILayoutUtility.GetRect(0f, WaveHeight, GUILayout.ExpandWidth(true));
            area = new Rect(area.x + 8f, area.y, area.width - 16f, area.height);
            EditorGUI.DrawRect(area, BackgroundColor);

            if (slots.Count == 0)
            {
                return;
            }

            float halfWidth = Mathf.Max(ScreenHalfWidth, slots.Max(slot => Mathf.Abs(slot.GridPosition.x))) + BoundsMargin;
            int minY = slots.Min(slot => slot.GridPosition.y);
            int maxY = slots.Max(slot => slot.GridPosition.y);

            DrawGuides(area, halfWidth);

            foreach (WaveConfigDTO.WaveFormationDTO slot in slots)
            {
                // Higher y spawns further above the screen, so it is drawn higher and lands later.
                float tx = Mathf.InverseLerp(-halfWidth, halfWidth, slot.GridPosition.x);
                float ty = maxY == minY ? 1f : Mathf.InverseLerp(minY, maxY, slot.GridPosition.y);

                Rect dot = new Rect(
                    area.x + (tx * area.width) - (SlotSize * 0.5f),
                    area.yMax - 14f - (ty * (area.height - 26f)) - (SlotSize * 0.5f),
                    SlotSize, SlotSize);

                bool offScreen = Mathf.Abs(slot.GridPosition.x) > ScreenHalfWidth;
                EditorGUI.DrawRect(dot, offScreen ? OffScreenColor : ColorFor(slot.EnemyType));
            }
        }

        /// <summary>Draws the arrival line the entry animation tweens to, and the screen edges.</summary>
        private void DrawGuides(Rect area, float halfWidth)
        {
            EditorGUI.DrawRect(new Rect(area.x, area.yMax - 8f, area.width, 1f), ArrivalLineColor);

            float leftEdge = Mathf.InverseLerp(-halfWidth, halfWidth, -ScreenHalfWidth);
            float rightEdge = Mathf.InverseLerp(-halfWidth, halfWidth, ScreenHalfWidth);

            EditorGUI.DrawRect(new Rect(area.x + (leftEdge * area.width), area.y, 1f, area.height), ScreenEdgeColor);
            EditorGUI.DrawRect(new Rect(area.x + (rightEdge * area.width), area.y, 1f, area.height), ScreenEdgeColor);
        }

        private static string DescribeMix(List<WaveConfigDTO.WaveFormationDTO> slots)
        {
            IEnumerable<IGrouping<EnemyTypes, WaveConfigDTO.WaveFormationDTO>> groups =
                slots.GroupBy(slot => slot.EnemyType).OrderBy(group => group.Key);

            return $"{slots.Count} enemies ({string.Join(", ", groups.Select(group => $"{group.Key} x{group.Count()}"))})";
        }

        private static Color ColorFor(EnemyTypes enemyType)
        {
            switch (enemyType)
            {
                case EnemyTypes.Enemy1:
                    return new Color(0.45f, 0.75f, 1f);
                case EnemyTypes.Enemy2:
                    return new Color(1f, 0.62f, 0.25f);
                case EnemyTypes.Boss1:
                    return new Color(1f, 0.35f, 0.75f);
                default:
                    return Color.white;
            }
        }
    }
}
