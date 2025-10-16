using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Preload
{
    public class BootModel : Model
    {
        public float AnimationSimulationTimerSeconds { get; } = 3f;
        public float AnimationEndDelayTimerSeconds { get; } = 1f;
    }
}