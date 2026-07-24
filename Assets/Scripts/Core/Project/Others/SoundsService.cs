using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Scenes.Game;
using SpaceInvaders.Scenes.MainMenu;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ISoundsService : IInitializable, IDisposable, IGameStartListener, IGameEndListener, IMenuEnterListener
    {
        void PlaySound(SoundTypes type);
        bool IsPlaying(SoundTypes type);
        bool IsCategoryPlaying(SoundCategory category);
    }

    public class SoundsService : ISoundsService
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ISoundsManager _soundsManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;

        public void Initialize()
        {
            _messageBus.Subscribe<ButtonClickedMessage>(OnButtonClicked);
            _messageBus.Subscribe<ShipShotFiredMessage>(OnShipShotFired);
            _messageBus.Subscribe<ShipDamagedMessage>(OnShipDamaged);
            _messageBus.Subscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Subscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
            _messageBus.Subscribe<PowerupActivatedMessage>(OnPowerupActivated);
            _messageBus.Subscribe<PowerupExpiredMessage>(OnPowerupExpired);
            _messageBus.Subscribe<BossSpawnedMessage>(OnBossSpawned);
            _messageBus.Subscribe<WaveStartedMessage>(OnWaveStarted);
            _messageBus.Subscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public void Dispose()
        {
            _messageBus.Unsubscribe<ButtonClickedMessage>(OnButtonClicked);
            _messageBus.Unsubscribe<ShipShotFiredMessage>(OnShipShotFired);
            _messageBus.Unsubscribe<ShipDamagedMessage>(OnShipDamaged);
            _messageBus.Unsubscribe<EnemyDestroyedMessage>(OnEnemyDestroyed);
            _messageBus.Unsubscribe<PlayerDestroyedMessage>(OnPlayerDestroyed);
            _messageBus.Unsubscribe<PowerupActivatedMessage>(OnPowerupActivated);
            _messageBus.Unsubscribe<PowerupExpiredMessage>(OnPowerupExpired);
            _messageBus.Unsubscribe<BossSpawnedMessage>(OnBossSpawned);
            _messageBus.Unsubscribe<WaveStartedMessage>(OnWaveStarted);
            _messageBus.Unsubscribe<LevelCompletedMessage>(OnLevelCompleted);
        }

        public UniTask MenuEnter()
        {
            PlaySound(SoundTypes.MenuMusic);
            return UniTask.CompletedTask;
        }

        public UniTask GameStart(int levelNumber)
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
            var config = _repositoryManager.GetSoundConfig(type);

            if(config == null)
            {
                this.LogWarning($"No sound could be found for {type}");
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
            return _soundsManager.IsPlaying(_repositoryManager.GetSoundConfig(type).Clip);
        }

        public bool IsCategoryPlaying(SoundCategory category)
        {
            return _soundsManager.IsChannelPlaying(category.ToString());
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

        private void OnPowerupActivated(PowerupActivatedMessage message)
        {
            PlaySound(SoundTypes.PowerupPickup);
        }

        private void OnPowerupExpired(PowerupExpiredMessage message)
        {
            PlaySound(SoundTypes.PowerupExpired);
        }

        private void OnBossSpawned(BossSpawnedMessage message)
        {
            PlaySound(SoundTypes.BossSpawned);
        }

        private void OnWaveStarted(WaveStartedMessage message)
        {
            PlaySound(SoundTypes.WaveStarted);
        }

        private void OnLevelCompleted(LevelCompletedMessage message)
        {
            PlaySound(SoundTypes.LevelCompleted);
        }
    }
}
