using System;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPlayerManager : IInitializable, IDisposable, IGameplayStateHandler
    {
    }

    public class PlayerManager : IPlayerManager
    {
        [Inject] private readonly IInputService _inputService;
        [Inject] private readonly ISpawnService _spawnService;
        [Inject] private readonly GameObject _playerPrefab;

        private Transform _playerInstance;

        public void Initialize()
        {
            _playerInstance = _spawnService.Spawn(_playerPrefab, _playerPrefab.transform.localPosition, _playerPrefab.transform.localRotation).transform;
        }

        public void OnGameStarted()
        {
            _inputService.OnShoot += OnPlayerShoot;
            _inputService.OnMove += OnPlayerMove;
        }

        public void OnGameEnded()
        {
            _inputService.OnShoot -= OnPlayerShoot;
            _inputService.OnMove -= OnPlayerMove;
        }

        public void Dispose()
        {
            DespawnPlayer();
        }
        
        private void OnPlayerShoot()
        {
            // Handle player shoot logic
        }

        private void OnPlayerMove(Vector3 direction)
        {
            _playerInstance.Translate(direction);
        }

        private void DespawnPlayer()
        {
            if (_playerInstance == null)
            {
                return;
            }

            _spawnService.Despawn(_playerInstance.gameObject);
            _playerInstance = null;
        }
    }
}
