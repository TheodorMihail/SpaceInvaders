using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum PlayerTypes
    {
        Player1
    }
    
    public interface IPlayerManager : IDisposable, IGameStartListener, IGameEndListener, IGameInitializeListener
    {
        Vector3 PlayerPosition { get; }
        ShipStats PlayerStats { get; }
    }

    public class PlayerManager : IPlayerManager, ITickable
    {
        [Inject] private readonly ISpawnService _spawnService;
        [Inject] private readonly IMessageBus _messageBus;

        private IPlayerSpaceship _playerInstance;

        public Vector3 PlayerPosition => _playerInstance != null ? _playerInstance.Position : Vector3.zero;
        public ShipStats PlayerStats => _playerInstance?.Stats;

        
        public async UniTask GameInitialize()
        {
            _playerInstance = await _spawnService.SpawnPlayer();
            _playerInstance.OnDestroyed += OnDestroyedCallback;
        }

        public UniTask GameStart(int levelNumber)
        {
            _playerInstance.EnableControls();
            return UniTask.CompletedTask;
        }

        public UniTask GameEnd()
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
            _messageBus.Publish(new PlayerDestroyedMessage());
        }

        private void DespawnPlayer()
        {
            if (_playerInstance == null)
            {
                return;
            }

            _playerInstance.OnDestroyed -= OnDestroyedCallback;
            _spawnService.Despawn(_playerInstance as PlayerSpaceshipBehaviourComponent);
            _playerInstance = null;
        }

        #region Debugging

        public void Tick()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
            {
                this.LogWarning("Debug: Destroying player");
                _playerInstance.TakeDamage(_playerInstance.Stats.CurrentHealth);
            }
#endif
        }

        #endregion
    }
}
