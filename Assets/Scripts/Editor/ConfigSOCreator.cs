using System.IO;
using SpaceInvaders.Project;
using UnityEditor;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>Keyboard-shortcut variants of the config CreateAssetMenu entries, for quickly
    /// authoring templates without going through the Create submenu each time.</summary>
    public static class ConfigSOCreator
    {
        [MenuItem("Assets/Create/SpaceInvaders/Items/New Item Config %#i")]
        private static void CreateItemConfig()
        {
            CreateConfig<ItemConfigSO>("ItemConfig");
        }

        [MenuItem("Assets/Create/SpaceInvaders/Sounds/New Sound Config %#u")]
        private static void CreateSoundConfig()
        {
            CreateConfig<SoundConfigSO>("SoundConfig");
        }

        private static void CreateConfig<T>(string fileName) where T : ScriptableObject
        {
            string folderPath = GetSelectedFolderPath();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{fileName}.asset");

            var config = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        private static string GetSelectedFolderPath()
        {
            foreach (Object selected in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                string path = AssetDatabase.GetAssetPath(selected);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (AssetDatabase.IsValidFolder(path))
                {
                    return path;
                }

                string containingFolder = Path.GetDirectoryName(path)?.Replace('\\', '/');
                if (!string.IsNullOrEmpty(containingFolder))
                {
                    return containingFolder;
                }
            }

            return "Assets";
        }
    }
}
