using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>
    /// Shared helpers for turning a sliced sprite sheet into animation clips.
    /// </summary>
    internal static class SpriteAnimationUtils
    {
        private const string OutputFolderName = "Animations";

        public static bool TryGetSelectedTexturePath(out string texturePath)
        {
            texturePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            return IsTexture(texturePath);
        }

        public static bool IsTexture(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && AssetImporter.GetAtPath(assetPath) is TextureImporter;
        }

        /// <summary>Every sprite the slicer produced, in frame order. A name token narrows it to one
        /// animation on a sheet that holds several.</summary>
        public static List<Sprite> LoadSprites(string texturePath, string nameToken = null)
        {
            return AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .Where(sprite => string.IsNullOrEmpty(nameToken) || sprite.name.Contains(nameToken))
                .OrderBy(TrailingNumber)
                .ToList();
        }

        /// <summary>Rewrites the pivot of every slice on the sheet. Frames sliced automatically each
        /// get their own bounding box, so a shared pivot is what stops the animation drifting about.</summary>
        public static void SetPivots(string texturePath, SpriteAlignment alignment)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

#pragma warning disable CS0618 // The sprite data provider API is far heavier for a one-off pivot fix.
            SpriteMetaData[] sheet = importer.spritesheet;
            Vector2 pivot = PivotFor(alignment);

            for (int i = 0; i < sheet.Length; i++)
            {
                sheet[i].alignment = (int)alignment;
                sheet[i].pivot = pivot;
            }

            importer.spritesheet = sheet;
#pragma warning restore CS0618

            importer.SaveAndReimport();
        }

        public static string CreateOutputFolder(string texturePath)
        {
            string parentFolder = Path.GetDirectoryName(texturePath).Replace('\\', '/');
            string outputFolder = $"{parentFolder}/{OutputFolderName}";

            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                AssetDatabase.CreateFolder(parentFolder, OutputFolderName);
            }

            return outputFolder;
        }

        /// <summary>An empty binding path means the Animator sits on the same object as the renderer,
        /// which is how the animated prefabs here are built.</summary>
        public static AnimationClip CreateClip(List<Sprite> sprites, string assetPath, float frameRate, bool loop)
        {
            var clip = new AnimationClip { frameRate = frameRate };

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, assetPath);

            return clip;
        }

        /// <summary>Frame order comes from the trailing index, so "_10" sorts after "_9".</summary>
        private static int TrailingNumber(Sprite sprite)
        {
            Match match = Regex.Match(sprite.name, @"(\d+)$");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private static Vector2 PivotFor(SpriteAlignment alignment)
        {
            return alignment switch
            {
                SpriteAlignment.TopLeft => new Vector2(0f, 1f),
                SpriteAlignment.TopCenter => new Vector2(0.5f, 1f),
                SpriteAlignment.TopRight => new Vector2(1f, 1f),
                SpriteAlignment.LeftCenter => new Vector2(0f, 0.5f),
                SpriteAlignment.RightCenter => new Vector2(1f, 0.5f),
                SpriteAlignment.BottomLeft => new Vector2(0f, 0f),
                SpriteAlignment.BottomCenter => new Vector2(0.5f, 0f),
                SpriteAlignment.BottomRight => new Vector2(1f, 0f),
                _ => new Vector2(0.5f, 0.5f)
            };
        }
    }
}
