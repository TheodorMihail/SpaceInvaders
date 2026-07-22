using System;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IInputService : ITickable
    {
        event Action OnShoot;
        event Action<Vector3> OnMove;
        event Action OnAnyKeyPress;
    }
}
