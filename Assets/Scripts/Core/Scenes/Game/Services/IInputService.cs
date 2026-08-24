using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Reads one platform's devices. Raising anything from it is the manager's job.</summary>
    public interface IInputService
    {
        bool AnyKeyPressed { get; }
        bool PausePressed { get; }
        bool ShootPressed { get; }

        /// <summary>Zero while the player is not steering.</summary>
        Vector3 MoveDirection { get; }
    }
}
