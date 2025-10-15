using Base.Systems;

namespace SpaceInvaders.Scenes.GamePreload
{
    public class SplashModel : Model
    {
        public float AnimationSimulationTimerSeconds { get; } = 2f;
        public float AnimationStartDelayTimerSeconds { get; } = 0.5f;
        public float AnimationEndDelayTimerSeconds { get; } = 1f;
    }
}