using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Stretches its own rect over the play area instead of the screen. Opt-in: left off the UI that
    /// should stay at the screen edges.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PlayAreaFitterUIComponent : MonoBehaviour
    {
        [Inject] private readonly ICameraManager _cameraManager;

        /// <summary>Start rather than Awake: the play area is not answerable until injection has run.</summary>
        private void Start()
        {
            SetupPlayArea();
        }

        private void SetupPlayArea()
        {
            var rectTransform = (RectTransform)transform;
            Rect playfieldRect = _cameraManager.PlayfieldViewportRect;

            rectTransform.anchorMin = playfieldRect.min;
            rectTransform.anchorMax = playfieldRect.max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
