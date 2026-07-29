using System;
using System.Collections.Generic;
using SpaceInvaders.Scenes.Game;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace SpaceInvaders.Editor
{
    public class LevelGeneratorWindow : EditorWindow
    {
        private const string LevelsFolderPath = "Assets/GameAssets/Configs/Levels";
        private const string ConfigsContainerPath = "Assets/GameAssets/Configs/ConfigsContainer.asset";
        private const int MinEnemyTypeCount = 0;
        private const int MaxEnemyTypeCount = 40;
        private const int MaxWaveCount = 20;

        [SerializeField] private int _levelNumber = 1;
        [SerializeField] private LevelTypes _levelType;
        [SerializeField] private int _numberOfWaves = 1;
        [SerializeField] private int _spacingX = 50;
        [SerializeField] private int _spacingY = 100;
        [SerializeField] private bool _useFixedSeed;
        [SerializeField] private int _seed;
        [SerializeField] private List<WaveDraft> _waveDrafts = new List<WaveDraft>();

        private Vector2 _scrollPosition;
        private string _validationError;

        [Serializable]
        private class WaveDraft
        {
            public bool FoldoutExpanded = true;
            public float TimeBetweenSpawns = 1f;
            public float EntrySpeed = 100f;
            public int[] EnemyTypeCounts;
        }

        [MenuItem("SpaceInvaders/Level Generator")]
        private static void ShowWindow()
        {
            LevelGeneratorWindow window = GetWindow<LevelGeneratorWindow>("Level Generator");
            window.minSize = new Vector2(420f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            ResizeWaveDrafts();
        }

        private void OnGUI()
        {
            DrawLevelSettings();
            DrawSpacingSettings();
            DrawWavesSection();
            DrawSeedSection();
            DrawValidationAndGenerateButton();
        }

        private void DrawLevelSettings()
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
            _levelNumber = Mathf.Max(1, EditorGUILayout.IntField("Level Number", _levelNumber));
            _levelType = (LevelTypes)EditorGUILayout.EnumPopup("Level Type", _levelType);
            EditorGUILayout.Space();
        }

        private void DrawSpacingSettings()
        {
            EditorGUILayout.LabelField("Formation Spacing", EditorStyles.boldLabel);
            _spacingX = Mathf.Max(10, EditorGUILayout.IntField("Spacing X", _spacingX));
            _spacingY = Mathf.Max(10, EditorGUILayout.IntField("Spacing Y", _spacingY));
            EditorGUILayout.Space();
        }

        private void DrawWavesSection()
        {
            EditorGUILayout.LabelField("Waves", EditorStyles.boldLabel);

            int newWaveCount = Mathf.Clamp(EditorGUILayout.IntField("Number of Waves", _numberOfWaves), 1, MaxWaveCount);
            if (newWaveCount != _numberOfWaves)
            {
                _numberOfWaves = newWaveCount;
                ResizeWaveDrafts();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(200f));
            for (int i = 0; i < _waveDrafts.Count; i++)
            {
                DrawWaveDraft(i);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
        }

        private void DrawWaveDraft(int index)
        {
            WaveDraft draft = _waveDrafts[index];
            EnemyTypes[] enemyTypeValues = (EnemyTypes[])Enum.GetValues(typeof(EnemyTypes));

            EditorGUILayout.BeginVertical("box");
            draft.FoldoutExpanded = EditorGUILayout.Foldout(draft.FoldoutExpanded, $"Wave {index + 1}", true);

            if (draft.FoldoutExpanded)
            {
                EditorGUI.indentLevel++;
                draft.TimeBetweenSpawns = Mathf.Max(0f, EditorGUILayout.FloatField("Time Between Spawns", draft.TimeBetweenSpawns));
                draft.EntrySpeed = Mathf.Max(0f, EditorGUILayout.FloatField("Entry Speed", draft.EntrySpeed));

                EditorGUILayout.LabelField("Enemy Counts");
                int total = 0;
                for (int typeIndex = 0; typeIndex < enemyTypeValues.Length; typeIndex++)
                {
                    draft.EnemyTypeCounts[typeIndex] = Mathf.Clamp(
                        EditorGUILayout.IntField(enemyTypeValues[typeIndex].ToString(), draft.EnemyTypeCounts[typeIndex]),
                        MinEnemyTypeCount, MaxEnemyTypeCount);
                    total += draft.EnemyTypeCounts[typeIndex];
                }
                EditorGUILayout.LabelField("Total Enemies", total.ToString());
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSeedSection()
        {
            EditorGUILayout.LabelField("Randomization", EditorStyles.boldLabel);
            _useFixedSeed = EditorGUILayout.Toggle("Use Fixed Seed", _useFixedSeed);

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!_useFixedSeed))
            {
                _seed = EditorGUILayout.IntField("Seed", _seed);
                if (GUILayout.Button("Randomize Seed", GUILayout.Width(120f)))
                {
                    _seed = Guid.NewGuid().GetHashCode();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("The seed actually used for Generate is always saved on the resulting asset (Generator Info section in its Inspector) and copied back into this field afterward, so any level can be reproduced later by checking Use Fixed Seed and entering that value.", MessageType.Info);
            EditorGUILayout.Space();
        }

        private void DrawValidationAndGenerateButton()
        {
            bool isValid = Validate();
            if (!string.IsNullOrEmpty(_validationError))
            {
                EditorGUILayout.HelpBox(_validationError, MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(!isValid))
            {
                if (GUILayout.Button("Generate Level", GUILayout.Height(30f)))
                {
                    GenerateLevel();
                }
            }
        }

        private bool Validate()
        {
            if (_levelNumber < 1)
            {
                _validationError = "Level Number must be 1 or greater.";
                return false;
            }

            for (int i = 0; i < _waveDrafts.Count; i++)
            {
                if (SumEnemyTypeCounts(_waveDrafts[i]) <= 0)
                {
                    _validationError = $"Wave {i + 1} must have at least one enemy across its enemy type counts.";
                    return false;
                }
            }

            _validationError = null;
            return true;
        }

        private static int SumEnemyTypeCounts(WaveDraft draft)
        {
            int total = 0;
            for (int i = 0; i < draft.EnemyTypeCounts.Length; i++)
            {
                total += draft.EnemyTypeCounts[i];
            }

            return total;
        }

        private void ResizeWaveDrafts()
        {
            while (_waveDrafts.Count < _numberOfWaves)
            {
                _waveDrafts.Add(CreateWaveDraft());
            }

            if (_waveDrafts.Count > _numberOfWaves)
            {
                _waveDrafts.RemoveRange(_numberOfWaves, _waveDrafts.Count - _numberOfWaves);
            }

            int enumLength = Enum.GetValues(typeof(EnemyTypes)).Length;
            for (int i = 0; i < _waveDrafts.Count; i++)
            {
                int[] existing = _waveDrafts[i].EnemyTypeCounts;
                if (existing != null && existing.Length == enumLength)
                {
                    continue;
                }

                int[] resized = new int[enumLength];
                if (existing != null)
                {
                    for (int j = 0; j < existing.Length && j < resized.Length; j++)
                    {
                        resized[j] = existing[j];
                    }
                }

                _waveDrafts[i].EnemyTypeCounts = resized;
            }
        }

        private static WaveDraft CreateWaveDraft()
        {
            return new WaveDraft
            {
                EnemyTypeCounts = new int[Enum.GetValues(typeof(EnemyTypes)).Length]
            };
        }

        private void GenerateLevel()
        {
            int effectiveSeed = _useFixedSeed ? _seed : Environment.TickCount ^ Guid.NewGuid().GetHashCode();
            Random random = new Random(effectiveSeed);
            List<WaveConfigDTO> wavesConfigs = BuildWavesConfigs(random);

            EnsureLevelsFolderExists();
            string assetPath = $"{LevelsFolderPath}/LevelConfig_{_levelNumber}.asset";
            LevelConfigSO existingAsset = AssetDatabase.LoadAssetAtPath<LevelConfigSO>(assetPath);
            LevelConfigSO targetAsset;

            if (existingAsset != null)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Overwrite Level Config",
                    $"An asset already exists at:\n{assetPath}\n\nOverwrite it?",
                    "Overwrite", "Cancel");

                if (!confirmed)
                {
                    return;
                }

                LevelConfigAssetWriter.ApplyToAsset(existingAsset, _levelNumber, _levelType, wavesConfigs, effectiveSeed);
                EditorUtility.SetDirty(existingAsset);
                targetAsset = existingAsset;
            }
            else
            {
                LevelConfigSO newAsset = CreateInstance<LevelConfigSO>();
                LevelConfigAssetWriter.ApplyToAsset(newAsset, _levelNumber, _levelType, wavesConfigs, effectiveSeed);
                AssetDatabase.CreateAsset(newAsset, assetPath);
                targetAsset = newAsset;
            }

            RegisterInConfigsContainer(targetAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(targetAsset);
            Selection.activeObject = targetAsset;

            _useFixedSeed = true;
            _seed = effectiveSeed;
        }

        private List<WaveConfigDTO> BuildWavesConfigs(Random random)
        {
            EnemyTypes[] enemyTypeValues = (EnemyTypes[])Enum.GetValues(typeof(EnemyTypes));
            FormationTemplateTypes[] templateValues = (FormationTemplateTypes[])Enum.GetValues(typeof(FormationTemplateTypes));
            List<WaveConfigDTO> wavesConfigs = new List<WaveConfigDTO>();

            foreach (WaveDraft draft in _waveDrafts)
            {
                List<EnemyTypes> enemyPool = new List<EnemyTypes>();
                for (int i = 0; i < enemyTypeValues.Length; i++)
                {
                    for (int c = 0; c < draft.EnemyTypeCounts[i]; c++)
                    {
                        enemyPool.Add(enemyTypeValues[i]);
                    }
                }
                Shuffle(enemyPool, random);

                FormationTemplateTypes template = templateValues[random.Next(templateValues.Length)];
                List<Vector2Int> slotPositions = FormationTemplateGenerator.Generate(template, enemyPool.Count, _spacingX, _spacingY, random);

                List<WaveConfigDTO.WaveFormationDTO> waveFormation = new List<WaveConfigDTO.WaveFormationDTO>();
                for (int i = 0; i < slotPositions.Count; i++)
                {
                    waveFormation.Add(new WaveConfigDTO.WaveFormationDTO
                    {
                        Position = slotPositions[i],
                        EnemyType = enemyPool[i]
                    });
                }

                wavesConfigs.Add(LevelConfigAssetWriter.BuildWaveConfig(waveFormation, draft.TimeBetweenSpawns, draft.EntrySpeed));
            }

            return wavesConfigs;
        }

        private static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        private static void EnsureLevelsFolderExists()
        {
            if (AssetDatabase.IsValidFolder(LevelsFolderPath))
            {
                return;
            }

            string[] segments = LevelsFolderPath.Split('/');
            string currentPath = segments[0];

            for (int i = 1; i < segments.Length; i++)
            {
                string nextPath = $"{currentPath}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }

                currentPath = nextPath;
            }
        }

        private static void RegisterInConfigsContainer(LevelConfigSO asset)
        {
            ConfigsContainerSO container = AssetDatabase.LoadAssetAtPath<ConfigsContainerSO>(ConfigsContainerPath);

            if (container == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:ConfigsContainerSO");
                if (guids.Length > 0)
                {
                    container = AssetDatabase.LoadAssetAtPath<ConfigsContainerSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (container == null)
            {
                Debug.LogWarning($"[LevelGenerator] Could not find a ConfigsContainerSO asset. '{asset.LevelName}' was created but must be manually added to the container's Level Configs list.");
                return;
            }

            if (container.LevelsDataConfigSO.LevelsConfigs.Contains(asset))
            {
                return;
            }

            container.LevelsDataConfigSO.LevelsConfigs.Add(asset);
            EditorUtility.SetDirty(container.LevelsDataConfigSO);
        }
    }
}
