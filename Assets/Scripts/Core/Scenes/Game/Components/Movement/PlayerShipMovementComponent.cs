namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Movement steered from outside, one frame at a time. It never moves on its own, so the ship
    /// calls <see cref="BaseShipMovementComponent.Move"/> directly as input arrives.
    /// </summary>
    public class PlayerShipMovementComponent : BaseShipMovementComponent
    {
        public override void StartMoving()
        {
            // Nothing to enable: input already drives this component directly.
        }

        public override void Tick()
        {
            // Nothing to advance: a frame without input is a frame standing still.
        }
    }
}
