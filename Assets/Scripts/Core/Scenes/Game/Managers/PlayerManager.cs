using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum PlayerTypes
    {
        Player1
    }
    
    public interface IPlayerManager : IDisposable, IGameStartListener, IGameEndListener, IGameInitializeListener
    {
        Vector3 PlayerLocalPosition { get; }

        /// <summary>False once the player is gone, so anything aiming falls back deliberately instead
        /// of tracking the origin a bare position getter would hand back.</summary>
        bool TryGetPlayerWorldPosition(out Vector3 worldPosition);
        ShipStats PlayerStats { get; }
    }

    public partial class PlayerManager : IPlayerManager, IInitializable
    {
        [Inject] private readonly ISpawnManager _spawnManager;
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;

        private IPlayerSpaceship _playerInstance;

        public Vector3 PlayerLocalPosition => _playerInstance != null ? _playerInstance.LocalPosition : Vector3.zero;
        public ShipStats PlayerStats => _playerInstance?.Stats;

        public void Initialize()
        {
            _messageBus.Subscribe<GamePausedMessage>(OnGamePaused);
            _messageBus.Subscribe<GameResumedMessage>(OnGameResumed);
            _messageBus.Subscribe<AllEnemiesDestroyedMessage>(OnAllEnemiesDestroyed);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<GamePausedMessage>(OnGamePaused);
            _messageBus.Unsubscribe<GameResumedMessage>(OnGameResumed);
            _messageBus.Unsubscribe<AllEnemiesDestroyedMessage>(OnAllEnemiesDestroyed);

            DespawnPlayer();
        }

        /// <summary>Spawns the player ship and applies permanent progression bonuses to its stats.
        /// Controls are enabled later, on game start.</summary>
        public async UniTask GameInitialize()
        {
            _playerInstance = await _spawnManager.SpawnPlayer();
            _talentManager.ApplyTalentBonuses(_playerInstance.Stats);
            _equipmentManager.ApplyEquipmentBonuses(_playerInstance.Stats);
            _playerInstance.OnDestroyed += OnDestroyedCallback;
            _playerInstance.OnShotFired += OnShotFiredCallback;
            _playerInstance.OnDamaged += OnDamagedCallback;
            _playerInstance.OnAmmoChanged += OnAmmoChangedCallback;
            _playerInstance.OnReloadStarted += OnReloadStartedCallback;
        }

        public UniTask GameStart(int levelNumber)
        {
            _playerInstance.EnableControls();
            return UniTask.CompletedTask;
        }

        public bool TryGetPlayerWorldPosition(out Vector3 worldPosition)
        {
            if (_playerInstance == null)
            {
                worldPosition = Vector3.zero;
                return false;
            }

            worldPosition = _playerInstance.WorldPosition;
            return true;
        }

        public UniTask GameEnd()
        {
            DespawnPlayer();
            return UniTask.CompletedTask;
        }

        /// <summary>Input keeps ticking while the game is paused, so controls are detached explicitly.</summary>
        private void OnGamePaused(GamePausedMessage message)
        {
            _playerInstance?.DisableControls();
        }

        private void OnGameResumed(GameResumedMessage message)
        {
            _playerInstance?.EnableControls();
        }

        /// <summary>The lull between waves is free time, so the player starts the next one topped up
        /// rather than having to burn the opening seconds reloading.</summary>
        private void OnAllEnemiesDestroyed(AllEnemiesDestroyedMessage message)
        {
            _playerInstance?.Reload();
        }

        private void OnDestroyedCallback(IPlayerSpaceship component)
        {
            this.Log($"Player destroyed!");
            DespawnPlayer();
            _messageBus.Publish(new PlayerDestroyedMessage());
        }

        private void OnShotFiredCallback(ISpaceship spaceship)
        {
            _messageBus.Publish(new ShipShotFiredMessage(spaceship.LocalPosition));
        }

        private void OnDamagedCallback(ISpaceship spaceship, int damage, bool isCritical)
        {
            _messageBus.Publish(new ShipDamagedMessage(spaceship.Stats.CurrentHealth, damage, isCritical, spaceship.WorldPosition, isPlayer: true));
        }

        private void OnAmmoChangedCallback(int currentAmmo, int maxAmmo)
        {
            _messageBus.Publish(new PlayerAmmoChangedMessage(currentAmmo, maxAmmo));
        }

        private void OnReloadStartedCallback(float duration)
        {
            _messageBus.Publish(new PlayerReloadStartedMessage(duration));
        }

        private void DespawnPlayer()
        {
            if (_playerInstance == null)
            {
                return;
            }

            _playerInstance.OnDestroyed -= OnDestroyedCallback;
            _playerInstance.OnShotFired -= OnShotFiredCallback;
            _playerInstance.OnDamaged -= OnDamagedCallback;
            _playerInstance.OnAmmoChanged -= OnAmmoChangedCallback;
            _playerInstance.OnReloadStarted -= OnReloadStartedCallback;
            _spawnManager.Despawn(_playerInstance as PlayerSpaceshipBehaviourComponent);
            _playerInstance = null;
        }
    }
}
