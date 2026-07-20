using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPowerupManager : IDisposable, IInitializable, IGameEndListener
    {
        event Action<PowerupTypes, float> PowerupActivated; // duration = 0 means instant
        event Action<PowerupTypes> PowerupExpired;

        void ActivatePowerup(PowerupTypes type);
    }

    public class PowerupManager : IPowerupManager
    {
        [Inject] private readonly IRepositoryManager _repositoryManager;
        [Inject] private readonly IPlayerManager _playerManager;
        [Inject] private readonly IEnemiesManager _enemiesManager;
        [Inject] private readonly ISpawnService _spawnService;
        [Inject] private readonly ICustomFactory _factory;

        private readonly Dictionary<PowerupTypes, IPowerupBehaviour> _activePowerups = new();

        public event Action<PowerupTypes, float> PowerupActivated;
        public event Action<PowerupTypes> PowerupExpired;

        public void Initialize()
        {
            _enemiesManager.EnemyDestroyed += OnEnemyDestroyed;
        }

        public void Dispose()
        {
            _enemiesManager.EnemyDestroyed -= OnEnemyDestroyed;
            ClearActivePowerups();
        }

        public UniTask GameEnd()
        {
            ClearActivePowerups();
            return UniTask.CompletedTask;
        }

        private void OnEnemyDestroyed(string enemyId, Vector3 position)
        {
            if (TryGetPowerupDrop(out var config))
            {
                _spawnService.SpawnPowerup(config, position);
            }
        }

        private bool TryGetPowerupDrop(out PowerupConfigSO config)
        {
            config = null;

            if (Random.value > _repositoryManager.GetPowerupDropChance())
            {
                return false;
            }

            var configs = _repositoryManager.GetAllPowerupConfigs();

            if (configs.Count == 0)
            {
                return false;
            }

            int totalWeight = 0;
            foreach (var candidate in configs)
            {
                totalWeight += candidate.DropWeight;
            }

            if (totalWeight <= 0)
            {
                return false;
            }

            int roll = Random.Range(0, totalWeight);

            foreach (var candidate in configs)
            {
                roll -= candidate.DropWeight;
                if (roll < 0)
                {
                    config = candidate;
                    return true;
                }
            }

            return false;
        }

        public void ActivatePowerup(PowerupTypes type)
        {
            var config = _repositoryManager.GetPowerupConfig(type);

            if (_activePowerups.TryGetValue(type, out var existing))
            {
                existing.Refresh();
                PowerupActivated?.Invoke(type, config.Duration);
                return;
            }

            var powerup = CreatePowerup(type);
            powerup.Initialize(_playerManager.PlayerStats, config);
            PowerupActivated?.Invoke(type, config.Duration);

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
            PowerupExpired?.Invoke(powerup.PowerupType);
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
