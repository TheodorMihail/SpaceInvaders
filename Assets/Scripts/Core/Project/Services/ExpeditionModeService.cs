using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// A run through the map. Progression is the same machinery as Campaign's, only against the
    /// Expedition save profile, so this holds nothing of its own.
    /// </summary>
    public class ExpeditionModeService : IGameModeService
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;

        public GameModeTypes Mode => GameModeTypes.Expedition;
        public SceneTypes HubScene => SceneTypes.Expedition;

        /// <summary>Carried health is set after the bonuses, which decide the maximum.</summary>
        public void ApplyProgressionBonuses(ShipStats stats)
        {
            _talentManager.ApplyTalentBonuses(stats);
            _equipmentManager.ApplyEquipmentBonuses(stats);

            stats.SetHealthRatio(_expeditionRunManager.RemainingHealthRatio);
        }

        public void SaveLevelResult(GameSessionDTO session, ShipStats stats)
        {
            _expeditionRunManager.CompleteCurrentNode(stats);
        }

        /// <summary>Only a node played out pays, and any other ending closes the run.</summary>
        public void SaveRunScore(GameSessionResultDTO result, int score)
        {
            switch (result.Result)
            {
                case GameplayStateResultTypes.LevelFinished:
                    _expeditionRunManager.BankScrap(score);
                    break;
                case GameplayStateResultTypes.GameOver:
                    _expeditionRunManager.EndRunInDefeat();
                    break;
                default:
                    _expeditionRunManager.AbandonRun();
                    break;
            }
        }

        /// <summary>No result screen: the map and the run summary report a node instead.</summary>
        public GameOverOptionTypes GetGameOverOptions(GameSessionResultDTO result)
        {
            return GameOverOptionTypes.None;
        }
    }
}
