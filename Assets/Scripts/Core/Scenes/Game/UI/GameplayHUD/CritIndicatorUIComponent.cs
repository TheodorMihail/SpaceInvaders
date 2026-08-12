using System;
using BaseArchitecture.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Screen space text marking a critical hit, drifting up as it fades out.</summary>
    public class CritIndicatorUIComponent : MonoBehaviour, IPoolableObject
    {
        private static readonly Vector2 CenterAnchor = new(0.5f, 0.5f);

        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _riseDistance = 60f;
        [SerializeField] private Ease _riseEase = Ease.OutCubic;
        [SerializeField] private Ease _fadeEase = Ease.InQuad;

        private RectTransform _rectTransform;
        private Sequence _sequence;

        public event Action<CritIndicatorUIComponent> OnFinished;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public void Initialize(string text, Vector3 screenPosition, float duration)
        {
            KillSequence();

            _text.text = text;
            _text.alpha = 1f;
            MoveTo(screenPosition);

            PlayRiseAndFade(duration);
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            KillSequence();
        }

        /// <summary>Pooled indicators are destroyed with the HUD, which can happen mid tween.</summary>
        private void OnDestroy()
        {
            KillSequence();
        }

        /// <summary>Re-anchors to the container centre first, so placement does not depend on how the
        /// prefab was authored, then converts the screen point into the container's local space.</summary>
        private void MoveTo(Vector3 screenPosition)
        {
            var container = _rectTransform.parent as RectTransform;

            if (container == null)
            {
                return;
            }

            _rectTransform.anchorMin = CenterAnchor;
            _rectTransform.anchorMax = CenterAnchor;

            // A null camera is correct here: the HUD canvas renders in screen space overlay.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screenPosition, null, out Vector2 localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }

        /// <summary>The tween owns the lifetime, so the indicator is only released once it has faded.</summary>
        private void PlayRiseAndFade(float duration)
        {
            Vector2 targetPosition = _rectTransform.anchoredPosition + (Vector2.up * _riseDistance);

            _sequence = DOTween.Sequence()
                .Join(DOTween.To(() => _rectTransform.anchoredPosition, position => _rectTransform.anchoredPosition = position, targetPosition, duration).SetEase(_riseEase))
                .Join(DOTween.To(() => _text.alpha, alpha => _text.alpha = alpha, 0f, duration).SetEase(_fadeEase))
                .SetTarget(_rectTransform)
                .OnComplete(Finish);
            _sequence.Play();
        }

        private void Finish()
        {
            _sequence = null;
            OnFinished?.Invoke(this);
        }

        private void KillSequence()
        {
            _sequence?.Kill();
            _sequence = null;
        }
    }
}
