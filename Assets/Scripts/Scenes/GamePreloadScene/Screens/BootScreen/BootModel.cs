using Base.Systems;

namespace SpaceInvaders.Scenes.GamePreload
{
    public class BootModel : Model
    {
        public float AnimationSimulationTimerSeconds { get; } = 3f;
        public float AnimationEndDelayTimerSeconds { get; } = 1f;
    }
}