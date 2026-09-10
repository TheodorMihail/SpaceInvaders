using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Project
{
    [CreateAssetMenu(fileName = "DropsDataConfig", menuName = "SpaceInvaders/Data Config/Drops Data Config")]
    public class DropsDataConfigSO : ScriptableObject
    {
        [Tooltip("One per mode that drops differently, each naming itself.")]
        [SerializeField] private List<DropTableConfigSO> _dropTableConfigs = new();

        public virtual List<DropTableConfigSO> DropTableConfigs => _dropTableConfigs;
    }
}
