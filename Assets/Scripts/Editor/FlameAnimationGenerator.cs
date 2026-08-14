using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>
    /// Turns an already sliced flame sheet into its idle/thrust clips and a controller that swaps
    /// between them. Sprites are grouped by the name the slicer gave them, so the sheet must use the
    /// "..._small_flame_N" / "..._large_flame_N" convention.
    /// </summary>
    public static class FlameAnimationGenerator
    {
        private const string IdleSpriteToken = "small_flame";
        private const string ThrustSpriteToken = "large_flame";

        private const string IdleStateName = "FlameIdle";
        private const string ThrustStateName = "FlameThrust";
        private const string ThrustingParameter = "IsThrusting";

        private const string OutputFolderName = "Animations";
        private const float FrameRate = 12f;
        private const float TransitionDuration = 0.15f;

        [MenuItem("SpaceInvaders/Generate Flame Animations")]
        private static void GenerateFromSelection()
        {
            if (!TryGetSelectedTexturePath(out string texturePath))
            {
                EditorUtility.DisplayDialog("Generate Flame Animations",
                    "Select the sliced flame sprite sheet in the Project window first.", "OK");
                return;
            }

            SetPivotsToTopCenter(texturePath);

            List<Sprite> idleSprites = LoadSprites(texturePath, IdleSpriteToken);
            List<Sprite> thrustSprites = LoadSprites(texturePath, ThrustSpriteToken);

            if (idleSprites.Count == 0 || thrustSprites.Count == 0)
            {
                Debug.LogError($"[FlameAnimationGenerator] Expected sprites named '{IdleSpriteToken}' and " +
                    $"'{ThrustSpriteToken}' in '{texturePath}'. Found {idleSprites.Count} and {thrustSprites.Count}.");
                return;
            }

            string outputFolder = CreateOutputFolder(texturePath);
            string prefix = Path.GetFileNameWithoutExtension(texturePath);

            AnimationClip idleClip = CreateClip(idleSprites, $"{outputFolder}/{prefix}_{IdleStateName}.anim");
            AnimationClip thrustClip = CreateClip(thrustSprites, $"{outputFolder}/{prefix}_{ThrustStateName}.anim");

            CreateController($"{outputFolder}/{prefix}_Flame.controller", idleClip, thrustClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FlameAnimationGenerator] Generated flame animations in '{outputFolder}'.");
        }

        private static bool TryGetSelectedTexturePath(out string texturePath)
        {
            texturePath = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(texturePath) && AssetImporter.GetAtPath(texturePath) is TextureImporter;
        }

        /// <summary>The flame hangs off its emitter, so every frame has to pivot at the top. A centered
        /// pivot would grow the longer thrust frames up into the ship as well as backwards.</summary>
        private static void SetPivotsToTopCenter(string texturePath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);

#pragma warning disable CS0618 // The sprite data provider API is far heavier for a one-off pivot fix.
            SpriteMetaData[] sheet = importer.spritesheet;

            for (int i = 0; i < sheet.Length; i++)
            {
                sheet[i].alignment = (int)SpriteAlignment.TopCenter;
                sheet[i].pivot = new Vector2(0.5f, 1f);
            }

            importer.spritesheet = sheet;
#pragma warning restore CS0618

            importer.SaveAndReimport();
        }

        private static List<Sprite> LoadSprites(string texturePath, string nameToken)
        {
            return AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .Where(sprite => sprite.name.Contains(nameToken))
                .OrderBy(TrailingNumber)
                .ToList();
        }

        /// <summary>Frame order comes from the trailing index, so "_10" sorts after "_9".</summary>
        private static int TrailingNumber(Sprite sprite)
        {
            Match match = Regex.Match(sprite.name, @"(\d+)$");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private static string CreateOutputFolder(string texturePath)
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
        /// which is how the flame prefab is built.</summary>
        private static AnimationClip CreateClip(List<Sprite> sprites, string assetPath)
        {
            var clip = new AnimationClip { frameRate = FrameRate };

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
                    time = i / FrameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, assetPath);

            return clip;
        }

        /// <summary>Both transitions are immediate: thrust has to answer the input on the frame it
        /// arrives, so neither side waits for its loop to finish.</summary>
        private static void CreateController(string assetPath, AnimationClip idleClip, AnimationClip thrustClip)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            controller.AddParameter(ThrustingParameter, AnimatorControllerParameterType.Bool);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            AnimatorState idleState = stateMachine.AddState(IdleStateName);
            idleState.motion = idleClip;

            AnimatorState thrustState = stateMachine.AddState(ThrustStateName);
            thrustState.motion = thrustClip;

            stateMachine.defaultState = idleState;

            AddTransition(idleState, thrustState, AnimatorConditionMode.If);
            AddTransition(thrustState, idleState, AnimatorConditionMode.IfNot);
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.AddCondition(mode, 0f, ThrustingParameter);
            transition.hasExitTime = false;
            transition.duration = TransitionDuration;
        }
    }
}
