using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPowerupManager : IDisposable, IGameEndListener
    {
        void ActivatePowerup(PowerupTypes type);
    }

    /// <summary>Owns the active timed powerups, one instance per type.</summary>
    public class PowerupManager : IPowerupManager
    {
        [Inject] private readonly IPowerupsRepository _powerupsRepository;
        [Inject] private readonly IPlayerManager _playerManager;
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly ICustomFactory _factory;

        private readonly Dictionary<PowerupTypes, IPowerupBehaviour> _activePowerups = new();

        public void Dispose()
        {
            ClearActivePowerups();
        }

        public UniTask GameEnd()
        {
            ClearActivePowerups();
            return UniTask.CompletedTask;
        }

        /// <summary>Activating an already active type refreshes its timer instead of stacking the
        /// bonus. Powerups without a duration are applied immediately and not tracked.</summary>
        public void ActivatePowerup(PowerupTypes type)
        {
            if (!_powerupsRepository.TryGetPowerupConfig(type, out var config))
            {
                return;
            }

            float duration = GetEffectiveDuration(config);

            if (_activePowerups.TryGetValue(type, out var existing))
            {
                existing.Refresh(duration);
                _messageBus.Publish(new PowerupActivatedMessage(type, duration));
                return;
            }

            var powerup = CreatePowerup(type);
            powerup.Initialize(_playerManager.PlayerStats, config, duration);
            _messageBus.Publish(new PowerupActivatedMessage(type, duration));

            if (duration > 0f)
            {
                powerup.Ended += OnPowerupEnded;
                _activePowerups[type] = powerup;
            }
        }

        /// <summary>The ship's powerup duration bonus only lengthens timed powerups, so an instant one
        /// stays instant however much of it is stacked up.</summary>
        private float GetEffectiveDuration(PowerupConfigSO config)
        {
            if (config.Duration <= 0f)
            {
                return config.Duration;
            }

            return config.Duration + _playerManager.PlayerStats.CurrentPowerupDuration;
        }

        private IPowerupBehaviour CreatePowerup(PowerupTypes type) => type switch
        {
            PowerupTypes.Invincibility => _factory.CreateNewObject<InvincibilityPowerup>(),
            PowerupTypes.Heal => _factory.CreateNewObject<HealPowerup>(),
            PowerupTypes.DamageBoost => _factory.CreateNewObject<DamageBoostPowerup>(),
            PowerupTypes.RapidFire => _factory.CreateNewObject<RapidFirePowerup>(),
            PowerupTypes.SpreadShot => _factory.CreateNewObject<SpreadShotPowerup>(),
            PowerupTypes.UnlimitedAmmo => _factory.CreateNewObject<UnlimitedAmmoPowerup>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        private void OnPowerupEnded(IPowerupBehaviour powerup)
        {
            powerup.Ended -= OnPowerupEnded;
            _activePowerups.Remove(powerup.PowerupType);
            _messageBus.Publish(new PowerupExpiredMessage(powerup.PowerupType));
        }

        private void ClearActivePowerups()
        {
            foreach (var powerup in _activePowerups.Values)
            {
                powerup.Ended -= OnPowerupEnded;
                powerup.CancelTimer();
            }

            _activePowerups.Clear();
        }
    }
}
