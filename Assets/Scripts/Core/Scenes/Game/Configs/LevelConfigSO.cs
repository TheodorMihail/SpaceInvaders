using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "SpaceInvaders/Level Config")]
    public class LevelConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Level Settings")]
        [SerializeField] private int _levelIndex;
        [SerializeField] private LevelTypes _levelType;
        [SerializeField] private List<WaveConfigDTO> _wavesConfigs;

        public virtual int Index => _levelIndex;
        public virtual string LevelName => $"Level {_levelIndex}";
        public virtual LevelTypes LevelType => _levelType;
        public virtual List<WaveConfigDTO> WavesConfigs => _wavesConfigs;

        public virtual string ObjectID => LevelName;
    }
}