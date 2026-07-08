using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
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

        private IPlayerSpaceship _playerInstance;

        public event Action OnPlayerDestroyed;

        
        public async UniTask OnGameInitialized()
        {
            _playerInstance = await _spawnService.SpawnPlayer();
            _playerInstance.OnDestroyed += OnDestroyedCallback;
        }

        public UniTask OnGameStarted()
        {
            _playerInstance.EnableControls();
            return UniTask.CompletedTask;
        }

        public UniTask OnGameEnded()
        {
            DespawnPlayer();
            return UniTask.CompletedTask;
        }
        
        public void Dispose()
        {
            DespawnPlayer();
        }

        private void OnDestroyedCallback(IPlayerSpaceship component)
        {
            this.Log($"Player destroyed!");
            DespawnPlayer();
            OnPlayerDestroyed?.Invoke();
        }

        private void DespawnPlayer()
        {
            if (_playerInstance == null) 
                return;

            _playerInstance.OnDestroyed -= OnDestroyedCallback;
            _spawnService.Despawn(_playerInstance as PlayerSpaceshipBehaviourComponent);
            _playerInstance = null;
        }

        #region Debugging

        public void Tick()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F2))
            {
                this.LogWarning("Debug: Destroying player");
                _playerInstance.TakeDamage(_playerInstance.CurrentHealth);
            }
#endif
        }

        #endregion
    }
}
