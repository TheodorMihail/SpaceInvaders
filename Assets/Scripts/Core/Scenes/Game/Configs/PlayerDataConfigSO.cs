using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "PlayerDataConfig", menuName = "SpaceInvaders/Data Config/Player Data Config")]
    public class PlayerDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [SerializeField] private List<PlayerSpaceshipConfigSO> _playerConfigs;

        public virtual List<PlayerSpaceshipConfigSO> PlayerConfigs => _playerConfigs;

        public string ObjectID => nameof(PlayerDataConfigSO);
    }
}
