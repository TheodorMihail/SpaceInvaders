using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Scenes.Game;
using SpaceInvaders.Scenes.MainMenu;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ISoundsService : IInitializable, IDisposable, IGameInitializeListener, IGameEndListener, IMenuEnterListener
    {
        void PlaySound(SoundTypes type);
        bool IsPlaying(SoundTypes type);
        bool IsCategoryPlaying(SoundCategoryTypes category);
        float GetCategoryVolume(SoundCategoryTypes category);
        void SetCategoryVolume(SoundCategoryTypes category, float volume);
    }

    /// <summary>Plays sounds in response to bus messages and lifecycle phases.</summary>
    public class SoundsService : ISoundsService
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ISoundsManager _soundsManager;
        [Inject] private readonly ISoundsRepository _soundsRepository;

        public void Initialize()
        {
            _messageBus.Subscribe<ButtonClickedMessage>(OnButtonClicked);

            _messageBus.Subscribe<ShipShotFiredMessage>(OnShipShotFired);
            _messageBus.Subscribe<ShipDamagedMessage>(OnShipDamaged);
            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Subscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);

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

            _messageBus.Unsubscribe<PowerupDroppedMessage>(OnPowerupDropped);
            _messageBus.Unsubscribe<PowerupActivatedMessage>(OnPowerupActivated);
            _messageBus.Unsubscribe<PowerupExpiredMessage>(OnPowerupExpired);

            _messageBus.Unsubscribe<ItemDroppedMessage>(OnItemDropped);
            _messageBus.Unsubscribe<ItemCollectedMessage>(OnItemCollected);

            _messageBus.Unsubscribe<WaveStartedMessage>(OnWaveStarted);
            _messageBus.Unsubscribe<BossSpawnedMessage>(OnBossSpawned);

            _messageBus.Unsubscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public UniTask MenuEnter()
        {
            PlaySound(SoundTypes.MenuMusic);
            return UniTask.CompletedTask;
        }

        public UniTask GameInitialize()
        {
            PlaySound(SoundTypes.GameplayMusic);
            return UniTask.CompletedTask;
        }

        public UniTask GameEnd()
        {
            PlaySound(SoundTypes.GameOver);
            return UniTask.CompletedTask;
        }

        public void PlaySound(SoundTypes type)
        {
            if (!_soundsRepository.TryGetSoundConfig(type, out var config))
            {
                return;
            }

            string channel = config.Category.ToString();

            if (config.Loop)
            {
                _soundsManager.PlayLoop(config.Clip, channel, config.Volume);
            }
            else
            {
                _soundsManager.PlayOneShot(config.Clip, channel, config.Volume);
            }
        }

        public bool IsPlaying(SoundTypes type)
        {
            if (!_soundsRepository.TryGetSoundConfig(type, out var config))
            {
                return false;
            }

            return _soundsManager.IsPlaying(config.Clip);
        }

        public bool IsCategoryPlaying(SoundCategoryTypes category)
        {
            return _soundsManager.IsChannelPlaying(category.ToString());
        }

        public float GetCategoryVolume(SoundCategoryTypes category)
        {
            return _soundsManager.GetChannelVolume(category.ToString());
        }

        public void SetCategoryVolume(SoundCategoryTypes category, float volume)
        {
            _soundsManager.SetChannelVolume(category.ToString(), volume);
        }

        private void OnButtonClicked(ButtonClickedMessage message)
        {
            PlaySound(SoundTypes.ButtonClick);
        }

        private void OnShipShotFired(ShipShotFiredMessage message)
        {
            PlaySound(SoundTypes.ShipShoot);
        }

        private void OnShipDamaged(ShipDamagedMessage message)
        {
            PlaySound(SoundTypes.ShipDamaged);
        }

        private void OnEnemyDestroyed(EnemyDestroyedMessage message)
        {
            PlaySound(SoundTypes.EnemyDestroyed);
        }

        private void OnPlayerDestroyed(PlayerDestroyedMessage message)
        {
            PlaySound(SoundTypes.PlayerDestroyed);
        }

        private void OnPowerupDropped(PowerupDroppedMessage message)
        {
            PlaySound(SoundTypes.PowerupDropped);
        }

        private void OnPowerupActivated(PowerupActivatedMessage message)
        {
            PlaySound(SoundTypes.PowerupPickup);
        }

        private void OnPowerupExpired(PowerupExpiredMessage message)
        {
            PlaySound(SoundTypes.PowerupExpired);
        }

        private void OnWaveStarted(WaveStartedMessage message)
        {
            PlaySound(SoundTypes.WaveStarted);
        }

        private void OnBossSpawned(BossSpawnedMessage message)
        {
            PlaySound(SoundTypes.BossSpawned);
        }

        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            PlaySound(SoundTypes.LevelCompleted);
        }

        private void OnItemDropped(ItemDroppedMessage message)
        {
            PlaySound(SoundTypes.ItemDropped);
        }

        private void OnItemCollected(ItemCollectedMessage message)
        {
            PlaySound(SoundTypes.ItemCollected);
        }
    }
}
