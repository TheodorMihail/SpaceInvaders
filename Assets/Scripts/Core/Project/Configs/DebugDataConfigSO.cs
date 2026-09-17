using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// What the cheats hand out. Deliberately not compiled out with the rest of the debug subsystem:
    /// a config class that disappears from a release build leaves its asset pointing at nothing, and
    /// the numbers cost nothing to ship. Only the code reading them is wrapped.
    /// </summary>
    [CreateAssetMenu(fileName = "DebugDataConfig", menuName = "SpaceInvaders/Data Config/Debug Data Config")]
    public class DebugDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Currency")]
        [SerializeField] private int _addCurrencyAmount = 999999;

        [Tooltip("Apart from the currency amount, since a run is spent on a far smaller scale.")]
        [SerializeField] private int _addScrapAmount = 5000;

        public virtual int AddCurrencyAmount => _addCurrencyAmount;
        public virtual int AddScrapAmount => _addScrapAmount;

        public string ObjectID => nameof(DebugDataConfigSO);
    }
}
