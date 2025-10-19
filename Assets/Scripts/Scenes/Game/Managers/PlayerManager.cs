using System;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPlayerManager : IInitializable, IDisposable
    {
    }

    public class PlayerManager : IPlayerManager, IGameStartedListener
    {
        [Inject] private readonly ISpawnService _spawnService;
        [Inject] private readonly ShipBehaviourComponent _playerPrefab;

        private ShipBehaviourComponent _playerInstance;


        public void Initialize()
        {
            _playerInstance = _spawnService.Spawn(_playerPrefab, _playerPrefab.transform.localPosition, _playerPrefab.transform.localRotation);
        }

        public void Dispose()
        {
            DespawnPlayer();
        }

        public void OnGameStarted()
        {
            _playerInstance.Initialize();
        }

        private void DespawnPlayer()
        {
            if (_playerInstance == null)
            {
                return;
            }

            _spawnService.Despawn(_playerInstance);
            _playerInstance = null;
        }
    }
}
