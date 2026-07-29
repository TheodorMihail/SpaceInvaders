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

    public class PowerupManager : IPowerupManager
    {
        [Inject] private readonly IRepositoryManager _repositoryManager;
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

        public void ActivatePowerup(PowerupTypes type)
        {
            var config = _repositoryManager.GetPowerupConfig(type);

            if (_activePowerups.TryGetValue(type, out var existing))
            {
                existing.Refresh();
                _messageBus.Publish(new PowerupActivatedMessage(type, config.Duration));
                return;
            }

            var powerup = CreatePowerup(type);
            powerup.Initialize(_playerManager.PlayerStats, config);
            _messageBus.Publish(new PowerupActivatedMessage(type, config.Duration));

            if (config.Duration > 0f)
            {
                powerup.Ended += OnPowerupEnded;
                _activePowerups[type] = powerup;
            }
        }

        private IPowerupBehaviour CreatePowerup(PowerupTypes type) => type switch
        {
            PowerupTypes.Invincibility => _factory.CreateNewObject<InvincibilityPowerup>(),
            PowerupTypes.Heal => _factory.CreateNewObject<HealPowerup>(),
            PowerupTypes.DamageBoost => _factory.CreateNewObject<DamageBoostPowerup>(),
            PowerupTypes.RapidFire => _factory.CreateNewObject<RapidFirePowerup>(),
            PowerupTypes.SpreadShot => _factory.CreateNewObject<SpreadShotPowerup>(),
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
