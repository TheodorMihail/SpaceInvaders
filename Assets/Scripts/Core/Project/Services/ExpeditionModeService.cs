using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// A run through the map. Progression is the same machinery as Campaign's, only against the
    /// Expedition save profile, so this holds nothing of its own.
    /// </summary>
    public class ExpeditionModeService : IGameModeService
    {
        public GameModeTypes Mode => GameModeTypes.Expedition;
        public SceneTypes HubScene => SceneTypes.Expedition;

        /// <summary>Nothing launches a level from the map yet, so the members below are never reached.</summary>
        public void ApplyProgressionBonuses(ShipStats stats)
        {
        }

        public void SaveLevelResult(GameSessionDTO session, ShipStats stats)
        {
        }

        /// <summary>Scrap is banked when a node is completed, so a run that ends in defeat pays nothing.</summary>
        public void SaveRunScore(GameSessionResultDTO result, int score)
        {
        }

        public GameOverOptionTypes GetGameOverOptions(GameSessionResultDTO result)
        {
            return GameOverOptionTypes.MainMenu;
        }
    }
}
