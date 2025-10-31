using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "SpaceInvaders/Level Config")]
    public class LevelConfigSO : ScriptableObject
    {
        [Header("Level Settings")]
        [SerializeField] private string _levelName;
        [SerializeField] private List<WaveConfigDTO> _wavesConfigs;

        public string LevelName => _levelName;
        public List<WaveConfigDTO> WavesConfigs => _wavesConfigs;
    }
}