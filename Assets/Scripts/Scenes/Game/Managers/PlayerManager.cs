using System;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPlayerManager : IInitializable
    {
        event Action OnPlayerDestroyed;
    }

    public class PlayerManager : IPlayerManager, IGameStartedListener
    {
        [Inject] private readonly ISpawnService _spawnService;
        [Inject] private readonly PlayerSpaceshipBehaviourComponent _playerPrefab;

        private PlayerSpaceshipBehaviourComponent _playerInstance;

        public event Action OnPlayerDestroyed;

        public void Initialize()
        {
            _playerInstance = _spawnService.Spawn(_playerPrefab, _playerPrefab.transform.localPosition, _playerPrefab.transform.localRotation);
            _playerInstance.OnDestroyed += OnDestroyedCallback;
        }

        public void OnGameStarted()
        {
            _playerInstance.EnableControls();
        }
        
        private void OnDestroyedCallback(SpaceshipBehaviourComponent component)
        {
            OnPlayerDestroyed?.Invoke();
        }
    }
}
