using System.IO;
using SpaceInvaders.Project;
using UnityEditor;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>Keyboard-shortcut variant of ItemConfigSO's CreateAssetMenu entry, for quickly
    /// authoring item templates without going through the Create submenu each time.</summary>
    public static class ItemConfigSOCreator
    {
        [MenuItem("Assets/Create/SpaceInvaders/Items/New Item Config %#i")]
        private static void CreateItemConfig()
        {
            string folderPath = GetSelectedFolderPath();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/ItemConfig.asset");

            var config = ScriptableObject.CreateInstance<ItemConfigSO>();
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
