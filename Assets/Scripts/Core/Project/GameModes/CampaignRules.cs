using System.Collections.Generic;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
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
        [Inject] private readonly IGameModesRepository _gameModesRepository;
        [Inject] private readonly ILevelProgressManager _levelProgressManager;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;
        [Inject] private readonly ICurrencyManager _currencyManager;

        public GameModeTypes Mode => GameModeTypes.Campaign;
        public SceneTypes HubScene => SceneTypes.Campaign;
        public IReadOnlyList<DropCategoryWeightDTO> DropWeights => _gameModesRepository.GetDataConfig(Mode)?.DropWeights;
        public bool CanReplayLevel => true;

        public void ApplyProgressionBonuses(ShipStats stats)
        {
            _talentManager.ApplyTalentBonuses(stats);
            _equipmentManager.ApplyEquipmentBonuses(stats);
        }

        /// <summary>A level is authored to be exactly as hard as it plays, so difficulty comes from
        /// which level was picked rather than from anything scaling underneath it.</summary>
        public float GetEnemyStatBonus(GameSessionDTO session)
        {
            return 0f;
        }

        /// <summary>The score is banked however the level ended, but only a cleared level is rated and
        /// only a cleared boss pays its bonus.</summary>
        public GameEndResolutionDTO ResolveGameEnd(GameSessionResultDTO result)
        {
            bool isCleared = result.Result == GameplayStateResultTypes.LevelFinished;
            LevelConfigSO levelConfig = isCleared ? GetLevelConfig(result.Session.LevelNumber) : null;

            BankCurrency(result.Score, levelConfig != null && levelConfig.LevelType == LevelTypes.Boss);

            if (isCleared && levelConfig != null)
            {
                RecordStars(result, levelConfig);
            }

            return new GameEndResolutionDTO(GetGameEndOptions(result));
        }

        /// <summary>Looked up once per ending, since both the rating and the boss bonus read it.</summary>
        private LevelConfigSO GetLevelConfig(int levelNumber)
        {
            _levelsRepository.TryGetLevelConfig(levelNumber, out LevelConfigSO config);
            return config;
        }

        private void BankCurrency(int score, bool isBossLevel)
        {
            GameModeDataConfigSO runDataConfig = _gameModesRepository.GetDataConfig(Mode);

            if (runDataConfig == null)
            {
                return;
            }

            int currency = Mathf.RoundToInt(score * runDataConfig.CurrencyPerScore);

            if (isBossLevel)
            {
                currency += runDataConfig.BossCurrencyBonus;
            }

            _currencyManager.AddCurrency(currency);
        }

        /// <summary>Stars come from the damage taken against the level's authored threshold.</summary>
        private void RecordStars(GameSessionResultDTO result, LevelConfigSO levelConfig)
        {
            if (result.Stats == null)
            {
                return;
            }

            int stars = CalculateStars(result.Stats.CumulativeDamageTaken, levelConfig.ThreeStarMaxDamage,
                _levelsRepository.GetTwoStarDamageMultiplier());

            _levelProgressManager.RecordLevelResult(result.Session.LevelNumber, stars);
        }

        /// <summary>Next Level is only offered on a cleared level with one left to advance to.</summary>
        private GameEndOptionTypes GetGameEndOptions(GameSessionResultDTO result)
        {
            GameEndOptionTypes options = GameEndOptionTypes.ReplayLevel | GameEndOptionTypes.MainMenu;

            if (result.Result == GameplayStateResultTypes.LevelFinished
                && result.Session.LevelNumber < _levelProgressManager.MaxLevelNumber)
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
