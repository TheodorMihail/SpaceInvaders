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

        public virtual List<PowerupConfigSO> PowerupConfigs => _powerupConfigs;

        public string ObjectID => nameof(PowerupsDataConfigSO);
    }
}
