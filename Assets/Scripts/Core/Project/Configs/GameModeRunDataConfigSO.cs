using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// What one mode drops and what it pays. Held per mode rather than in one shared table, so tuning
    /// a mode never reaches into another's numbers and adding a mode adds an asset, not a list entry.
    /// </summary>
    public abstract class GameModeRunDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Drops")]
        [Tooltip("Rolled on every enemy kill. Left empty, the mode drops nothing at all.")]
        [SerializeField] private DropTableConfigSO _dropTable;

        [Header("Currency")]
        [Tooltip("Banked per point of score. The mode decides what its currency is called.")]
        [SerializeField] private float _currencyPerScore = 1f;

        [Tooltip("Flat amount on top of the level's own, for clearing a boss.")]
        [SerializeField] private int _bossCurrencyBonus;

        public virtual DropTableConfigSO DropTable => _dropTable;
        public virtual float CurrencyPerScore => _currencyPerScore;
        public virtual int BossCurrencyBonus => _bossCurrencyBonus;

        public abstract GameModeTypes Mode { get; }

        public virtual string ObjectID => Mode.ToString();
    }
}
