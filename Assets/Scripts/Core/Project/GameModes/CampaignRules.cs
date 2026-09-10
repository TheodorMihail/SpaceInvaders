using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// The authored level list: talents and equipment on the ship, stars on completion, and the run's
    /// score banked as persistent currency.
    /// </summary>
    public class CampaignRules : IGameModeRules
    {
        [Inject] private readonly ILevelsRepository _levelsRepository;
        [Inject] private readonly ILevelProgressManager _levelProgressManager;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;

        public GameModeTypes Mode => GameModeTypes.Campaign;
        public SceneTypes HubScene => SceneTypes.Campaign;

        public void ApplyProgressionBonuses(ShipStats stats)
        {
            _talentManager.ApplyTalentBonuses(stats);
            _equipmentManager.ApplyEquipmentBonuses(stats);
        }

        /// <summary>The score is permanent currency however the level ended, but only a cleared level
        /// is rated.</summary>
        public GameEndResolutionDTO ResolveGameEnd(GameSessionResultDTO result)
        {
            _currencyManager.AddCurrency(result.Score);

            if (result.Result == GameplayStateResultTypes.LevelFinished)
            {
                RecordStars(result);
            }

            return new GameEndResolutionDTO(GetGameOverOptions(result));
        }

        /// <summary>Stars come from the damage taken against the level's authored threshold.</summary>
        private void RecordStars(GameSessionResultDTO result)
        {
            if (result.Stats == null
                || !_levelsRepository.TryGetLevelConfig(result.Session.LevelNumber, out LevelConfigSO config))
            {
                return;
            }

            int stars = CalculateStars(result.Stats.CumulativeDamageTaken, config.ThreeStarMaxDamage,
                _levelsRepository.GetTwoStarDamageMultiplier());

            _levelProgressManager.RecordLevelResult(result.Session.LevelNumber, stars);
        }

        /// <summary>Next Level is only offered while there is a level left to advance to.</summary>
        private GameEndOptionTypes GetGameOverOptions(GameSessionResultDTO result)
        {
            if (result.Result != GameplayStateResultTypes.LevelFinished)
            {
                return GameEndOptionTypes.Restart | GameEndOptionTypes.MainMenu;
            }

            GameEndOptionTypes options = GameEndOptionTypes.Retry | GameEndOptionTypes.MainMenu;

            if (result.Session.LevelNumber < _levelProgressManager.MaxLevelNumber)
            {
                options |= GameEndOptionTypes.NextLevel;
            }

            return options;
        }

        private static int CalculateStars(int damageTaken, int threeStarMaxDamage, float twoStarDamageMultiplier)
        {
            if (damageTaken <= threeStarMaxDamage)
            {
                return 3;
            }

            if (damageTaken <= threeStarMaxDamage * twoStarDamageMultiplier)
            {
                return 2;
            }

            return 1;
        }
    }
}
