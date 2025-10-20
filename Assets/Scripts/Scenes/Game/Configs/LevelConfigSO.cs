using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "SpaceInvaders/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        [Header("Level Settings")]
        [SerializeField] private int _levelNumber = 1;
        [SerializeField] private List<WaveConfigDTO> _wavesConfigs;

        public int LevelNumber => _levelNumber;
        public List<WaveConfigDTO> WavesConfigs => _wavesConfigs;
    }
}