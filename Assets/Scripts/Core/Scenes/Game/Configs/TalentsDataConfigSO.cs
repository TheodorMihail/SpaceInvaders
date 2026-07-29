using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "TalentsDataConfig", menuName = "SpaceInvaders/Data Config/Talents Data Config")]
    public class TalentsDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Talents")]
        [SerializeField] private List<TalentConfigSO> _talentConfigs;

        public virtual List<TalentConfigSO> TalentConfigs => _talentConfigs;

        public string ObjectID => nameof(TalentsDataConfigSO);
    }
}
