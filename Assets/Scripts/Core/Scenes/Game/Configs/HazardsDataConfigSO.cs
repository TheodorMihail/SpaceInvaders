using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "HazardsDataConfig", menuName = "SpaceInvaders/Data Config/Hazards Data Config")]
    public class HazardsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [SerializeField] private List<HazardConfigSO> _hazardConfigs;

        public virtual List<HazardConfigSO> HazardConfigs => _hazardConfigs;

        public string ObjectID => nameof(HazardsDataConfigSO);
    }
}
