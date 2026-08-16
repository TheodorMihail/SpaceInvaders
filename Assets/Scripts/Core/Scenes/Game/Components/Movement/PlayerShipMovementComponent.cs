namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Movement steered from outside, a frame at a time, by whoever is holding the controls. It never
    /// moves under its own power, so the ship calls <see cref="BaseShipMovementComponent.Move"/>
    /// directly as input arrives.
    /// </summary>
    public class PlayerShipMovementComponent : BaseShipMovementComponent
    {
        public override void StartMoving()
        {
            // Nothing to hand over: the controls were already driving this before the ship could move.
        }

        public override void Tick()
        {
            // Nothing to advance: a frame without input is a frame standing still.
        }
    }
}
