using System;
using System.Collections.Generic;
using System.Reflection;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Editor
{
    /// <summary>
    /// Writes the private serialized fields of the level configs by reflection, so the runtime types
    /// need no editor-only setters. Field lookups throw if the data model is renamed.
    /// </summary>
    internal static class LevelConfigAssetWriter
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly FieldInfo LevelIndexField = RequireField(typeof(LevelConfigSO), "_levelIndex");
        private static readonly FieldInfo LevelTypeField = RequireField(typeof(LevelConfigSO), "_levelType");
        private static readonly FieldInfo WavesConfigsField = RequireField(typeof(LevelConfigSO), "_wavesConfigs");
        private static readonly FieldInfo GeneratorSeedField = RequireField(typeof(LevelConfigSO), "_generatorSeed");

        private static readonly FieldInfo WavesFormationField = RequireField(typeof(WaveConfigDTO), "_wavesFormation");
        private static readonly FieldInfo TimeBetweenSpawnsField = RequireField(typeof(WaveConfigDTO), "_timeBetweenSpawns");
        private static readonly FieldInfo EntrySpeedField = RequireField(typeof(WaveConfigDTO), "_entrySpeed");

        public static void ApplyToAsset(LevelConfigSO asset, int levelIndex, LevelTypes levelType, List<WaveConfigDTO> wavesConfigs, int generatorSeed)
        {
            LevelIndexField.SetValue(asset, levelIndex);
            LevelTypeField.SetValue(asset, levelType);
            WavesConfigsField.SetValue(asset, wavesConfigs);
            GeneratorSeedField.SetValue(asset, generatorSeed);
        }

        public static WaveConfigDTO BuildWaveConfig(List<WaveConfigDTO.WaveFormationDTO> wavesFormation, float timeBetweenSpawns, float entrySpeed)
        {
            // Boxed because reflection would otherwise set the fields on a copy of the struct.
            object boxedWaveConfig = new WaveConfigDTO();
            WavesFormationField.SetValue(boxedWaveConfig, wavesFormation);
            TimeBetweenSpawnsField.SetValue(boxedWaveConfig, timeBetweenSpawns);
            EntrySpeedField.SetValue(boxedWaveConfig, entrySpeed);
            return (WaveConfigDTO)boxedWaveConfig;
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, PrivateInstance);
            if (field == null)
            {
                throw new MissingFieldException($"[LevelGenerator] Expected private field '{fieldName}' on '{type.FullName}' was not found. The runtime data model may have changed and this tool needs updating.");
            }

            return field;
        }
    }
}
