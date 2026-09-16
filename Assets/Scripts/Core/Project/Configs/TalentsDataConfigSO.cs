using System.Collections.Generic;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>One mode's talents, and anything true of them as a set rather than of a single
    /// talent.</summary>
    [CreateAssetMenu(fileName = "TalentsDataConfig", menuName = "SpaceInvaders/Talents/Talents Data Config")]
    public class TalentsDataConfigSO : ScriptableObject
    {
        [SerializeField] private List<TalentConfigSO> _talents = new();

        public virtual List<TalentConfigSO> Talents => _talents;
    }
}
