using DG.Tweening;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Briefly tints a sprite when it takes damage. Tints rather than brightens, because the sprite
    /// shaders multiply their colour and a white flash would be invisible on pale art.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlashComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        [Tooltip("Colour the hull is driven to on impact, then eased back from.")]
        [SerializeField] private Color _flashColor = new Color(1f, 0.35f, 0.3f, 1f);
        [SerializeField] private float _flashDuration = 0.12f;

        [Tooltip("Extra scale punched into the hull on impact. 0 turns the punch off.")]
        [SerializeField] private float _punchScale = 0.12f;

        private Color _restColor;
        private Vector3 _restScale;
        private Tween _colorTween;
        private Tween _scaleTween;

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            _restColor = _renderer.color;
            _restScale = transform.localScale;
        }

        /// <summary>Pooled ships can come back mid-flash, so the hull is put back before it is reused.</summary>
        private void OnDisable()
        {
            Restore();
        }

        public void Flash()
        {
            _colorTween?.Kill();
            _renderer.color = _flashColor;

            // DOTween's SpriteRenderer.DOColor lives in its Modules, which compile into
            // Assembly-CSharp and so cannot be referenced from this assembly. The core generic
            // tween is in the DLL and works regardless of how the modules are set up.
            _colorTween = DOTween
                .To(() => _renderer.color, color => _renderer.color = color, _restColor, _flashDuration)
                .SetEase(Ease.OutQuad);

            if (_punchScale <= 0f)
            {
                return;
            }

            _scaleTween?.Kill();
            transform.localScale = _restScale;
            _scaleTween = transform.DOPunchScale(_restScale * _punchScale, _flashDuration, vibrato: 1, elasticity: 0f);
        }

        private void Restore()
        {
            _colorTween?.Kill();
            _scaleTween?.Kill();
            _colorTween = null;
            _scaleTween = null;

            if (_renderer != null)
            {
                _renderer.color = _restColor;
            }

            transform.localScale = _restScale;
        }
    }
}
