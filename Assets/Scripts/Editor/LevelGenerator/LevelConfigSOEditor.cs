using SpaceInvaders.Scenes.Game;
using UnityEditor;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>Adds a read-only display of the generator seed used to build the asset.</summary>
    [CustomEditor(typeof(LevelConfigSO))]
    public class LevelConfigSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelConfigSO levelConfig = (LevelConfigSO)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generator Info", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Generator Seed", levelConfig.GeneratorSeed);
            }
        }
    }
}
