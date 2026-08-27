using System;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Which buttons the game over and victory screens offer. Flags, because a screen shows several at
    /// once and a plain enum could only carry one.
    /// </summary>
    [Flags]
    public enum GameOverOptionTypes
    {
        None = 0,
        Restart = 1,
        Retry = 2,
        NextLevel = 4,
        MainMenu = 8
    }

    /// <summary>
    /// Everything one game mode does differently. Private to the game mode manager, which holds the
    /// service for the running mode so nothing else branches on the mode.
    /// </summary>
    public interface IGameModeService
    {
        GameModeTypes Mode { get; }

        /// <summary>Scene to return to when a run is quit or finished.</summary>
        SceneTypes HubScene { get; }

        void ApplyProgressionBonuses(ShipStats stats);
        void SaveLevelResult(GameSessionDTO session, ShipStats stats);
        void SaveRunScore(GameSessionResultDTO result, int score);
        GameOverOptionTypes GetGameOverOptions(GameSessionResultDTO result);
    }
}
