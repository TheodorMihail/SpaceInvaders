using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Preload
{
    public class SplashModel : Model
    {
        public float AnimationSimulationTimerSeconds { get; } = 2f;
        public float AnimationStartDelayTimerSeconds { get; } = 0.5f;
        public float AnimationEndDelayTimerSeconds { get; } = 1f;
    }
}