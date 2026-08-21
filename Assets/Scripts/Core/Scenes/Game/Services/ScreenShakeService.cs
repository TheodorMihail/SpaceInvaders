using SpaceInvaders.Project;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public interface IScreenShakeService
    {
        /// <summary>How far the camera should currently sit from its mark.</summary>
        Vector3 Offset { get; }

        void Add(float amount);
        void Reset();

        /// <summary>Given the delta by its owner rather than reading a clock, so the owner decides
        /// whether the shake runs on scaled or unscaled time.</summary>
        void Tick(float deltaTime);
    }

    /// <summary>
    /// Works out how far the camera should be thrown. Bound only for the camera manager, so nothing
    /// else can reach past it to move the camera: this only computes an offset, and the manager stays
    /// the one thing that applies it.
    /// </summary>
    public class ScreenShakeService : IScreenShakeService
    {
        private readonly ImpactFeedbackSettings _settings;
        private readonly float _noiseSeed;

        private float _amount;

        public Vector3 Offset { get; private set; }

        public ScreenShakeService(IGameRepository gameRepository)
        {
            _settings = gameRepository.GetGameDataConfig().ImpactFeedback;
            _noiseSeed = Random.value * 100f;
        }

        /// <summary>Adds to whatever is already running rather than restarting it, so several impacts
        /// in one frame settle into a single shake instead of stacking into a lurch.</summary>
        public void Add(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _amount = Mathf.Clamp01(_amount + amount);
        }

        public void Reset()
        {
            _amount = 0f;
            Offset = Vector3.zero;
        }

        public void Tick(float deltaTime)
        {
            if (_amount <= 0f)
            {
                return;
            }

            _amount = Mathf.Max(_amount - _settings.ShakeDecayRate * deltaTime, 0f);

            if (_amount <= 0f)
            {
                Reset();
                return;
            }

            // Squared, so small knocks stay subtle while a boss landing throws the whole view.
            float magnitude = _amount * _amount * _settings.MaxShakeOffset;
            float time = Time.time * _settings.ShakeFrequency;

            // Noise rather than a random offset per frame, which reads as static instead of shake.
            Offset = new Vector3(
                (Mathf.PerlinNoise(_noiseSeed, time) * 2f - 1f) * magnitude,
                0f,
                (Mathf.PerlinNoise(_noiseSeed + 17f, time) * 2f - 1f) * magnitude);
        }
    }
}
