using System;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Which buttons a result screen offers, win or lose. Flags, because a screen shows several at once
    /// and a plain enum could only carry one. <see cref="None"/> means no screen at all.
    /// </summary>
    [Flags]
    public enum GameEndOptionTypes
    {
        None = 0,
        Restart = 1,
        Retry = 2,
        NextLevel = 4,
        MainMenu = 8
    }

    /// <summary>Where a finished level leads: the buttons offered, and what the hub scene is handed.</summary>
    public readonly struct GameEndResolutionDTO
    {
        public GameEndOptionTypes Options { get; }
        public object[] HubSceneParams => _hubSceneParams ?? Array.Empty<object>();

        private readonly object[] _hubSceneParams;

        public GameEndResolutionDTO(GameEndOptionTypes options, params object[] hubSceneParams)
        {
            Options = options;
            _hubSceneParams = hubSceneParams;
        }
    }

    /// <summary>
    /// How one game mode behaves. Owns nothing and stores nothing: the running mode's rules are held
    /// by the game mode manager, so nothing else branches on the mode.
    /// </summary>
    public interface IGameModeRules
    {
        GameModeTypes Mode { get; }

        /// <summary>Scene to return to when a run is quit or finished.</summary>
        SceneTypes HubScene { get; }

        void ApplyProgressionBonuses(ShipStats stats);

        /// <summary>The level ended. Everything it costs or earns is applied here, so nothing is left
        /// for a screen to apply later.</summary>
        GameEndResolutionDTO ResolveGameEnd(GameSessionResultDTO result);
    }
}
