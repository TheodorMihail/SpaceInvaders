using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// A run through the map. Progression is the same machinery as Campaign's, only against the
    /// Expedition save profile, so this holds nothing of its own.
    /// </summary>
    public class ExpeditionRules : IGameModeRules
    {
        [Inject] private readonly IExpeditionRunManager _expeditionRunManager;
        [Inject] private readonly ITalentManager _talentManager;
        [Inject] private readonly IEquipmentManager _equipmentManager;

        public GameModeTypes Mode => GameModeTypes.Expedition;
        public SceneTypes HubScene => SceneTypes.Expedition;

        /// <summary>Gear comes from the shop, so the table this names drops no items.</summary>
        public DropTableTypes DropTableType => DropTableTypes.Expedition;

        /// <summary>A node is consumed the moment it is entered, so there is nothing to replay.</summary>
        public bool CanReplayLevel => false;

        /// <summary>Carried health is set after the bonuses, which decide the maximum.</summary>
        public void ApplyProgressionBonuses(ShipStats stats)
        {
            _talentManager.ApplyTalentBonuses(stats);
            _equipmentManager.ApplyEquipmentBonuses(stats);

            stats.SetHealthRatio(_expeditionRunManager.CurrentExpedition?.RemainingHealthRatio ?? 1f);
        }

        /// <summary>Only a level played out pays, and any other ending closes the expedition. No result
        /// screen either way: the map and the expedition summary report it instead.</summary>
        public GameEndResolutionDTO ResolveGameEnd(GameSessionResultDTO result)
        {
            switch (result.Result)
            {
                case GameplayStateResultTypes.LevelFinished:
                    _expeditionRunManager.CompleteCurrentLevel(result);

                    if (_expeditionRunManager.CurrentExpedition?.IsOnFinalLevel ?? false)
                    {
                        return EndExpedition(ExpeditionRunResultTypes.Completed);
                    }

                    break;

                case GameplayStateResultTypes.GameOver:
                    return EndExpedition(ExpeditionRunResultTypes.Defeated);

                // Leaving a level is not a defeat, but the node is spent either way, so the expedition
                // is over and still worth reporting.
                default:
                    return EndExpedition(ExpeditionRunResultTypes.Abandoned);
            }

            return new GameEndResolutionDTO(GameEndOptionTypes.None);
        }

        /// <summary>The result rides the scene change, since nothing is left on disk to read it from.</summary>
        private GameEndResolutionDTO EndExpedition(ExpeditionRunResultTypes result)
        {
            return new GameEndResolutionDTO(GameEndOptionTypes.None, _expeditionRunManager.FinishExpedition(result));
        }
    }
}
