using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "PowerupsDataConfig", menuName = "SpaceInvaders/Powerups Data Config")]
    public class PowerupsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Powerups")]
        [SerializeField] private List<PowerupConfigSO> _powerupConfigs;

        [Header("Powerup Settings")]
        [SerializeField, Range(0f, 1f)] private float _globalPowerupDropChance = 0.15f;

        public virtual List<PowerupConfigSO> PowerupConfigs => _powerupConfigs;
        public virtual float GlobalPowerupDropChance => _globalPowerupDropChance;

        public string ObjectID => nameof(PowerupsDataConfigSO);
    }
}
