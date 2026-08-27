using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// The authored level list: talents and equipment on the ship, stars on completion, and the run's
    /// score banked as persistent currency.
    /// </summary>
    public class CampaignModeService : IGameModeService
    {
        [Inject] private readonly ILevelsRepository _levelsRepository;
        [Inject] private readonly ILevelProgressManager _levelProgressManager;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;

        public GameModeTypes Mode => GameModeTypes.Campaign;
        public SceneTypes HubScene => SceneTypes.MainMenu;

        public void ApplyProgressionBonuses(ShipStats stats)
        {
            _talentManager.ApplyTalentBonuses(stats);
            _equipmentManager.ApplyEquipmentBonuses(stats);
        }

        /// <summary>Stars come from the damage taken against the level's authored threshold.</summary>
        public void SaveLevelResult(GameSessionDTO session, ShipStats stats)
        {
            if (stats == null || !_levelsRepository.TryGetLevelConfig(session.LevelNumber, out LevelConfigSO config))
            {
                return;
            }

            int stars = CalculateStars(stats.CumulativeDamageTaken, config.ThreeStarMaxDamage,
                _levelsRepository.GetTwoStarDamageMultiplier());

            _levelProgressManager.RecordLevelResult(session.LevelNumber, stars);
        }

        public void SaveRunScore(GameSessionResultDTO result, int score)
        {
            _currencyManager.AddCurrency(score);
        }

        /// <summary>Next Level is only offered while there is a level left to advance to.</summary>
        public GameOverOptionTypes GetGameOverOptions(GameSessionResultDTO result)
        {
            if (result.Result != GameplayStateResultTypes.LevelFinished)
            {
                return GameOverOptionTypes.Restart | GameOverOptionTypes.MainMenu;
            }

            GameOverOptionTypes options = GameOverOptionTypes.Retry | GameOverOptionTypes.MainMenu;

            if (result.Session.LevelNumber < _levelProgressManager.MaxLevelNumber)
            {
                options |= GameOverOptionTypes.NextLevel;
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
