using System.Collections.Generic;
using System.IO;
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

        private const float FrameRate = 12f;
        private const float TransitionDuration = 0.15f;

        [MenuItem("SpaceInvaders/Animations/Generate Flame Animations")]
        private static void GenerateFromSelection()
        {
            if (!SpriteAnimationUtils.TryGetSelectedTexturePath(out string texturePath))
            {
                EditorUtility.DisplayDialog("Generate Flame Animations",
                    "Select the sliced flame sprite sheet in the Project window first.", "OK");
                return;
            }

            // The flame hangs off its emitter, so every frame has to pivot at the top. A centered
            // pivot would grow the longer thrust frames up into the ship as well as backwards.
            SpriteAnimationUtils.SetPivots(texturePath, SpriteAlignment.TopCenter);

            List<Sprite> idleSprites = SpriteAnimationUtils.LoadSprites(texturePath, IdleSpriteToken);
            List<Sprite> thrustSprites = SpriteAnimationUtils.LoadSprites(texturePath, ThrustSpriteToken);

            if (idleSprites.Count == 0 || thrustSprites.Count == 0)
            {
                Debug.LogError($"[FlameAnimationGenerator] Expected sprites named '{IdleSpriteToken}' and " +
                    $"'{ThrustSpriteToken}' in '{texturePath}'. Found {idleSprites.Count} and {thrustSprites.Count}.");
                return;
            }

            string outputFolder = SpriteAnimationUtils.CreateOutputFolder(texturePath);
            string prefix = Path.GetFileNameWithoutExtension(texturePath);

            AnimationClip idleClip = SpriteAnimationUtils.CreateClip(
                idleSprites, $"{outputFolder}/{prefix}_{IdleStateName}.anim", FrameRate, loop: true);
            AnimationClip thrustClip = SpriteAnimationUtils.CreateClip(
                thrustSprites, $"{outputFolder}/{prefix}_{ThrustStateName}.anim", FrameRate, loop: true);

            CreateController($"{outputFolder}/{prefix}_Flame.controller", idleClip, thrustClip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FlameAnimationGenerator] Generated flame animations in '{outputFolder}'.");
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
