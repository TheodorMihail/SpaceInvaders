using System;
using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum PlayerTypes
    {
        Player1
    }
    
    public interface IPlayerManager : IDisposable, IGameStartedListener, IGameEndedListener, IGameInitializeListener
    {
        event Action OnPlayerDestroyed;
    }

    public class PlayerManager : IPlayerManager, ITickable
    {
        [Inject] private readonly ISpawnService _spawnService;

        private PlayerSpaceshipBehaviourComponent _playerInstance;

        public event Action OnPlayerDestroyed;

        
        public async void OnGameInitialized()
        {
            _playerInstance = await _spawnService.SpawnPlayer();
            _playerInstance.OnDestroyed += OnDestroyedCallback;
        }

        public void OnGameStarted()
        {
            _playerInstance.EnableControls();
        }

        public void OnGameEnded()
        {
            DespawnPlayer();
        }
        
        public void Dispose()
        {
            DespawnPlayer();
        }

        private void OnDestroyedCallback(PlayerSpaceshipBehaviourComponent component)
        {
            this.Log($"Player destroyed!");
            DespawnPlayer();
            OnPlayerDestroyed?.Invoke();
        }

        private void DespawnPlayer()
        {
            _playerInstance.OnDestroyed -= OnDestroyedCallback;
            _spawnService.Despawn(_playerInstance);
        }

        #region  Debugging

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                this.LogWarning("Debug: Destroying player");
                _playerInstance.TakeDamage(_playerInstance.CurrentHealth);
            }
        }
        
        #endregion
    }
}
