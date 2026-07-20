using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "LevelsDataConfig", menuName = "SpaceInvaders/Levels Data Config")]
    public class LevelsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Levels")]
        [SerializeField] private List<LevelConfigSO> _levelsConfigs;

        [Header("Star Rating")]
        [SerializeField] private float _twoStarDamageMultiplier = 1.5f;

        public virtual List<LevelConfigSO> LevelsConfigs => _levelsConfigs;
        public virtual float TwoStarDamageMultiplier => _twoStarDamageMultiplier;

        public string ObjectID => nameof(LevelsDataConfigSO);
    }
}
