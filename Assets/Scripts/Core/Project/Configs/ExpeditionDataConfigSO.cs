using SpaceInvaders.Scenes.Expedition;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>Everything Expedition is made of. The parts that are only Expedition's hang here
    /// rather than off the shared container, so the mode is one place to start reading from.</summary>
    [CreateAssetMenu(fileName = "ExpeditionDataConfig", menuName = "SpaceInvaders/Game Modes/Expedition Data Config")]
    public class ExpeditionDataConfigSO : GameModeDataConfigSO
    {
        [Header("Expedition")]
        [SerializeField] private ExpeditionMapDataConfigSO _mapData;
        [SerializeField] private ExpeditionRewardsDataConfigSO _rewardsData;
        [SerializeField] private ExpeditionShopDataConfigSO _shopData;

        [Header("Difficulty")]
        [Tooltip("Added to enemy stats per depth beyond the first, so a run outgrows its own upgrades.")]
        [SerializeField] private float _enemyStatBonusPerDepth = 0.1f;

        public virtual ExpeditionMapDataConfigSO MapData => _mapData;
        public virtual ExpeditionRewardsDataConfigSO RewardsData => _rewardsData;
        public virtual ExpeditionShopDataConfigSO ShopData => _shopData;
        public virtual float EnemyStatBonusPerDepth => _enemyStatBonusPerDepth;

        public override GameModeTypes Mode => GameModeTypes.Expedition;
    }
}
