using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>One engine flame, on its own object under the ship so each hull can place and scale
    /// its engines independently.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShipFlameComponent : MonoBehaviour
    {
        private static readonly int ThrustingParameterHash = Animator.StringToHash("IsThrusting");

        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Scale")]
        [Tooltip("Multipliers on the scale authored on this object, so each hull keeps its own flame size.")]
        [SerializeField] private float _idleScaleMultiplier = 1f;
        [SerializeField] private float _thrustScaleMultiplier = 0.7f;

        [Tooltip("Seconds to grow into or shrink out of the thrust size. The sprite swap itself is instant.")]
        [SerializeField] private float _scaleTransitionDuration = 0.15f;

        private Vector3 _authoredScale;
        private float _currentMultiplier;
        private float _targetMultiplier;

        private void Awake()
        {
            _authoredScale = transform.localScale;
        }

        /// <summary>Pooled ships come back mid-transition, so the flame restarts at its idle size.</summary>
        private void OnEnable()
        {
            _currentMultiplier = _idleScaleMultiplier;
            _targetMultiplier = _idleScaleMultiplier;
            ApplyScale();
        }

        public void SetThrusting(bool isThrusting)
        {
            _animator.SetBool(ThrustingParameterHash, isThrusting);
            _targetMultiplier = isThrusting ? _thrustScaleMultiplier : _idleScaleMultiplier;
        }

        public void Show(bool show)
        {
            _renderer.enabled = show;
        }

        /// <summary>Scale is what sells the state change: two sprite sets cannot cross-fade into each
        /// other, so the size ramp carries the transition the swap cannot.</summary>
        private void Update()
        {
            if (Mathf.Approximately(_currentMultiplier, _targetMultiplier))
            {
                return;
            }

            if (_scaleTransitionDuration <= 0f)
            {
                _currentMultiplier = _targetMultiplier;
            }
            else
            {
                float speed = Mathf.Abs(_thrustScaleMultiplier - _idleScaleMultiplier) / _scaleTransitionDuration;
                _currentMultiplier = Mathf.MoveTowards(_currentMultiplier, _targetMultiplier, speed * Time.deltaTime);
            }

            ApplyScale();
        }

        private void ApplyScale()
        {
            transform.localScale = _authoredScale * _currentMultiplier;
        }
    }
}
