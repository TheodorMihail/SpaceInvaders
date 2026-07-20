using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "EnemyDataConfig", menuName = "SpaceInvaders/Enemy Data Config")]
    public class EnemyDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [SerializeField] private List<EnemySpaceshipConfigSO> _enemyConfigs;

        public virtual List<EnemySpaceshipConfigSO> EnemyConfigs => _enemyConfigs;

        public string ObjectID => nameof(EnemyDataConfigSO);
    }
}
