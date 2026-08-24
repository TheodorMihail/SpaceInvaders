using System;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ISoundsService
    {
        event Action<SoundTypes> OnSoundRequested;

        void Initialize();
        void Dispose();
    }

    /// <summary>Turns bus messages into the sound each one calls for.</summary>
    public class SoundsService : ISoundsService
    {
        [Inject] private readonly IMessageBus _messageBus;

        public event Action<SoundTypes> OnSoundRequested;

        public void Initialize()
        {
            _messageBus.Subscribe<ButtonClickedMessage>(OnButtonClicked);

            _messageBus.Subscribe<ShipShotFiredMessage>(OnShipShotFired);
            _messageBus.Subscribe<ShipDamagedMessage>(OnShipDamaged);
            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Subscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
            _messageBus.Subscribe<PlayerReloadStartedMessage>(OnPlayerReloadStarted);

            _messageBus.Subscribe<PowerupDroppedMessage>(OnPowerupDropped);
            _messageBus.Subscribe<PowerupActivatedMessage>(OnPowerupActivated);
            _messageBus.Subscribe<PowerupExpiredMessage>(OnPowerupExpired);

            _messageBus.Subscribe<ItemDroppedMessage>(OnItemDropped);
            _messageBus.Subscribe<ItemCollectedMessage>(OnItemCollected);

            _messageBus.Subscribe<WaveStartedMessage>(OnWaveStarted);
            _messageBus.Subscribe<BossSpawnedMessage>(OnBossSpawned);

            _messageBus.Subscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<ButtonClickedMessage>(OnButtonClicked);

            _messageBus.Unsubscribe<ShipShotFiredMessage>(OnShipShotFired);
            _messageBus.Unsubscribe<ShipDamagedMessage>(OnShipDamaged);
            _messageBus.Unsubscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Unsubscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
            _messageBus.Unsubscribe<PlayerReloadStartedMessage>(OnPlayerReloadStarted);

            _messageBus.Unsubscribe<PowerupDroppedMessage>(OnPowerupDropped);
            _messageBus.Unsubscribe<PowerupActivatedMessage>(OnPowerupActivated);
            _messageBus.Unsubscribe<PowerupExpiredMessage>(OnPowerupExpired);

            _messageBus.Unsubscribe<ItemDroppedMessage>(OnItemDropped);
            _messageBus.Unsubscribe<ItemCollectedMessage>(OnItemCollected);

            _messageBus.Unsubscribe<WaveStartedMessage>(OnWaveStarted);
            _messageBus.Unsubscribe<BossSpawnedMessage>(OnBossSpawned);

            _messageBus.Unsubscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        private void OnButtonClicked(ButtonClickedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.ButtonClick);
        }

        private void OnShipShotFired(ShipShotFiredMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.ShipShoot);
        }

        private void OnShipDamaged(ShipDamagedMessage message)
        {
            OnSoundRequested?.Invoke(message.IsCritical ? SoundTypes.ShipCriticalDamaged : SoundTypes.ShipDamaged);
        }

        private void OnEnemyDestroyed(EnemyDestroyedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.EnemyDestroyed);
        }

        private void OnPlayerDestroyed(PlayerDestroyedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.PlayerDestroyed);
        }

        private void OnPlayerReloadStarted(PlayerReloadStartedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.ShipReload);
        }

        private void OnPowerupDropped(PowerupDroppedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.PowerupDropped);
        }

        private void OnPowerupActivated(PowerupActivatedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.PowerupPickup);
        }

        private void OnPowerupExpired(PowerupExpiredMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.PowerupExpired);
        }

        private void OnItemDropped(ItemDroppedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.ItemDropped);
        }

        private void OnItemCollected(ItemCollectedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.ItemCollected);
        }

        private void OnWaveStarted(WaveStartedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.WaveStarted);
        }

        private void OnBossSpawned(BossSpawnedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.BossSpawned);
        }

        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            OnSoundRequested?.Invoke(SoundTypes.LevelCompleted);
        }
    }
}
