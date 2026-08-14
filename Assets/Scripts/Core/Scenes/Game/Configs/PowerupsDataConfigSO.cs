using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "PowerupsDataConfig", menuName = "SpaceInvaders/Data Config/Powerups Data Config")]
    public class PowerupsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Powerups")]
        [SerializeField] private List<PowerupConfigSO> _powerupConfigs;

        [Tooltip("Shared by every powerup; the powerup's icon is applied to it on spawn.")]
        [SerializeField] private PowerupBehaviourComponent _powerupPickupPrefab;

        public virtual List<PowerupConfigSO> PowerupConfigs => _powerupConfigs;
        public virtual PowerupBehaviourComponent PowerupPickupPrefab => _powerupPickupPrefab;

        public string ObjectID => nameof(PowerupsDataConfigSO);
    }
}
