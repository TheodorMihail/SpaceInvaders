using System;
using BaseArchitecture.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Impact feedback tuning. Screen shake accumulates rather than replaying, so several impacts in
    /// one frame combine into a single shake.
    /// </summary>
    [Serializable]
    public class ImpactFeedbackSettings
    {
        [Header("Screen Shake")]
        [Tooltip("How far the camera can be thrown at full shake, in world units. The play area is " +
                 "hundreds of units across, so anything single digit will not be visible at all.")]
        [SerializeField] private float _maxShakeOffset = 15f;

        [FormerlySerializedAs("_traumaDecayRate")]
        [Tooltip("Shake lost per second. Higher settles the camera sooner.")]
        [SerializeField] private float _shakeDecayRate = 1.8f;

        [Tooltip("How fast the shake oscillates.")]
        [SerializeField] private float _shakeFrequency = 22f;

        [Header("Screen Shake Per Event")]
        [Tooltip("Ordinary kills deliberately get none: waves are large enough that shaking per kill " +
                 "reads as noise rather than impact.")]
        [FormerlySerializedAs("_playerDamagedTrauma")]
        [SerializeField, Range(0f, 1f)] private float _playerDamagedShake = 0.45f;

        [FormerlySerializedAs("_playerDestroyedTrauma")]
        [SerializeField, Range(0f, 1f)] private float _playerDestroyedShake = 1f;

        [FormerlySerializedAs("_bossSpawnedTrauma")]
        [FormerlySerializedAs("_bossEnteredTrauma")]
        [SerializeField, Range(0f, 1f)] private float _bossEnteredShake = 0.6f;

        [FormerlySerializedAs("_bossDestroyedTrauma")]
        [SerializeField, Range(0f, 1f)] private float _bossDestroyedShake = 1f;

        [FormerlySerializedAs("_hazardDestroyedTrauma")]
        [SerializeField, Range(0f, 1f)] private float _hazardDestroyedShake = 0.3f;

        [Header("Slow Motion")]
        [Tooltip("Time scale held while slow motion runs. Reserved for the beats worth dwelling on, " +
                 "rather than ordinary impacts, which would make the game feel like it was stuttering.")]
        [SerializeField, Range(0.05f, 1f)] private float _slowMotionTimeScale = 0.3f;

        [Tooltip("Seconds of slow motion, measured in real time. 0 opts the event out.")]
        [SerializeField] private float _playerDestroyedSlowMotion = 1.2f;
        [SerializeField] private float _levelCompletedSlowMotion = 1f;

        [Tooltip("Runs when the boss lands on its mark, not when it spawns: at spawn it is still off " +
                 "screen with its whole entry ahead of it.")]
        [FormerlySerializedAs("_bossSpawnedSlowMotion")]
        [SerializeField] private float _bossEnteredSlowMotion = 0.8f;

        public float MaxShakeOffset => _maxShakeOffset;
        public float ShakeDecayRate => _shakeDecayRate;
        public float ShakeFrequency => _shakeFrequency;
        public float PlayerDamagedShake => _playerDamagedShake;
        public float PlayerDestroyedShake => _playerDestroyedShake;
        public float BossEnteredShake => _bossEnteredShake;
        public float BossDestroyedShake => _bossDestroyedShake;
        public float HazardDestroyedShake => _hazardDestroyedShake;
        public float SlowMotionTimeScale => _slowMotionTimeScale;
        public float PlayerDestroyedSlowMotion => _playerDestroyedSlowMotion;
        public float LevelCompletedSlowMotion => _levelCompletedSlowMotion;
        public float BossEnteredSlowMotion => _bossEnteredSlowMotion;
    }

    [CreateAssetMenu(fileName = "GameDataConfig", menuName = "SpaceInvaders/Data Config/Game Data Config")]
    public class GameDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Camera")]
        [Tooltip("Screen edges reserved for UI, as a fraction of the view, so ships never move underneath it.")]
        [SerializeField, Range(0f, 0.4f)] private float _sideMarginRatio = 0.05f;
        [SerializeField, Range(0f, 0.4f)] private float _topMarginRatio = 0.1f;
        [SerializeField, Range(0f, 0.4f)] private float _bottomMarginRatio = 0.03f;

        [Tooltip("Viewport height where the enemy region ends and the player's begins. Below the middle gives enemy formations more depth.")]
        [SerializeField, Range(0.1f, 0.9f)] private float _regionDividerRatio = 0.4f;

        [Header("Impact Feedback")]
        [SerializeField] private ImpactFeedbackSettings _impactFeedback = new();

        public virtual float SideMarginRatio => _sideMarginRatio;
        public virtual float TopMarginRatio => _topMarginRatio;
        public virtual float BottomMarginRatio => _bottomMarginRatio;
        public virtual float RegionDividerRatio => _regionDividerRatio;
        public virtual ImpactFeedbackSettings ImpactFeedback => _impactFeedback;

        public string ObjectID => nameof(GameDataConfigSO);
    }
}
