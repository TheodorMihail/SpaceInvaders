using System;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IGameplayManager : IInitializable, IDisposable
    {
    }

    public class GameplayManager : IGameplayManager
    {
        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}
