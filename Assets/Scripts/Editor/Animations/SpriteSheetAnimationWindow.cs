using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>
    /// Editor window that turns every slice on a sprite sheet into one looping clip, for sheets
    /// holding a single animation. Sheets with several animations need a dedicated generator.
    /// </summary>
    public class SpriteSheetAnimationWindow : EditorWindow
    {
        private const float MinFrameRate = 1f;
        private const float MaxFrameRate = 60f;

        [SerializeField] private Texture2D _spriteSheet;
        [SerializeField] private string _clipName = string.Empty;
        [SerializeField] private float _frameRate = 12f;
        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _overridePivot;
        [SerializeField] private SpriteAlignment _pivot = SpriteAlignment.BottomCenter;
        [SerializeField] private bool _createController = true;

        private string _validationError;

        [MenuItem("SpaceInvaders/Animations/Sprite Sheet Animation")]
        private static void ShowWindow()
        {
            SpriteSheetAnimationWindow window = GetWindow<SpriteSheetAnimationWindow>("Sprite Sheet Animation");
            window.minSize = new Vector2(380f, 260f);
            window.Show();
        }

        /// <summary>Opens on whatever is selected, since that is nearly always the sheet being worked on.</summary>
        private void OnEnable()
        {
            if (_spriteSheet == null)
            {
                _spriteSheet = Selection.activeObject as Texture2D;
            }
        }

        private void OnGUI()
        {
            DrawSourceSection();
            DrawClipSettings();
            DrawPivotSection();
            DrawGenerateSection();
        }

        private void DrawSourceSection()
        {
            EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);

            Texture2D previous = _spriteSheet;
            _spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet", _spriteSheet, typeof(Texture2D), false);

            if (_spriteSheet != previous)
            {
                _clipName = string.Empty;
            }

            int frameCount = GetFrameCount();
            EditorGUILayout.LabelField("Frames Found", frameCount.ToString());

            if (_spriteSheet != null && frameCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No slices on this texture. Set its Sprite Mode to Multiple and slice it in the " +
                    "Sprite Editor first.", MessageType.Warning);
            }

            EditorGUILayout.Space();
        }

        private void DrawClipSettings()
        {
            EditorGUILayout.LabelField("Clip", EditorStyles.boldLabel);
            _clipName = EditorGUILayout.TextField("Name", _clipName);
            _frameRate = Mathf.Clamp(EditorGUILayout.FloatField("Frame Rate", _frameRate), MinFrameRate, MaxFrameRate);
            _loop = EditorGUILayout.Toggle("Loop", _loop);
            _createController = EditorGUILayout.Toggle("Create Controller", _createController);
            EditorGUILayout.Space();
        }

        private void DrawPivotSection()
        {
            EditorGUILayout.LabelField("Pivot", EditorStyles.boldLabel);
            _overridePivot = EditorGUILayout.Toggle("Override Pivot", _overridePivot);

            using (new EditorGUI.DisabledScope(!_overridePivot))
            {
                _pivot = (SpriteAlignment)EditorGUILayout.EnumPopup("Alignment", _pivot);
            }

            if (_overridePivot)
            {
                EditorGUILayout.HelpBox(
                    "Rewrites the pivot of every slice and reimports the texture. Worth doing for sheets " +
                    "sliced automatically, where each frame gets its own bounding box and a shared pivot " +
                    "is what keeps the animation from drifting.", MessageType.Info);
            }

            EditorGUILayout.Space();
        }

        private void DrawGenerateSection()
        {
            if (!string.IsNullOrEmpty(_validationError))
            {
                EditorGUILayout.HelpBox(_validationError, MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(_spriteSheet == null))
            {
                if (GUILayout.Button("Generate", GUILayout.Height(30f)))
                {
                    Generate();
                }
            }
        }

        private int GetFrameCount()
        {
            if (_spriteSheet == null)
            {
                return 0;
            }

            string texturePath = AssetDatabase.GetAssetPath(_spriteSheet);
            return SpriteAnimationUtils.IsTexture(texturePath) ? SpriteAnimationUtils.LoadSprites(texturePath).Count : 0;
        }

        private void Generate()
        {
            _validationError = null;
            string texturePath = AssetDatabase.GetAssetPath(_spriteSheet);

            if (!SpriteAnimationUtils.IsTexture(texturePath))
            {
                _validationError = "The selected asset is not a texture.";
                return;
            }

            // Pivots move before the sprites are loaded, so the clip keys the reimported ones.
            if (_overridePivot)
            {
                SpriteAnimationUtils.SetPivots(texturePath, _pivot);
            }

            List<Sprite> sprites = SpriteAnimationUtils.LoadSprites(texturePath);

            if (sprites.Count == 0)
            {
                _validationError = "This texture has no slices. Slice it in the Sprite Editor first.";
                return;
            }

            string outputFolder = SpriteAnimationUtils.CreateOutputFolder(texturePath);
            string clipName = string.IsNullOrWhiteSpace(_clipName)
                ? Path.GetFileNameWithoutExtension(texturePath)
                : _clipName.Trim();

            AnimationClip clip = SpriteAnimationUtils.CreateClip(
                sprites, $"{outputFolder}/{clipName}.anim", _frameRate, _loop);

            if (_createController)
            {
                AnimatorController.CreateAnimatorControllerAtPathWithClip($"{outputFolder}/{clipName}.controller", clip);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(clip);
            Debug.Log($"[SpriteSheetAnimationWindow] Generated '{clipName}' from {sprites.Count} frames in '{outputFolder}'.");
        }
    }
}
