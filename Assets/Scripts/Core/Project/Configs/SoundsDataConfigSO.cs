using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    [CreateAssetMenu(fileName = "SoundsDataConfig", menuName = "SpaceInvaders/Data Config/Sounds Data Config")]
    public class SoundsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [SerializeField] private List<SoundConfigSO> _soundConfigs;

        public virtual List<SoundConfigSO> SoundConfigs => _soundConfigs;
        public string ObjectID => nameof(SoundsDataConfigSO);
    }
}
