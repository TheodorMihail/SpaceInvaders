using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "SpaceInvaders/Level Config")]
    public class LevelConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Level Settings")]
        [SerializeField] private string _levelName;
        [SerializeField] private LevelTypes _levelType;
        [SerializeField] private List<WaveConfigDTO> _wavesConfigs;

        public string LevelName => _levelName;
        public LevelTypes LevelType => _levelType;
        public List<WaveConfigDTO> WavesConfigs => _wavesConfigs;

        public string ObjectID => _levelType.ToString();
    }
}