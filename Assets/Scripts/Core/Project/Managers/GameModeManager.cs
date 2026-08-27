using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IGameModeManager : IInitializable
    {
        GameModeTypes CurrentMode { get; }
        SceneTypes HubScene { get; }

        /// <summary>Called once on entering the Game scene. The mode never changes within a run.</summary>
        void InitializeGameMode(GameModeTypes mode);

        void ApplyProgressionBonuses(ShipStats stats);
        void SaveLevelResult(GameSessionDTO session, ShipStats stats);
        void SaveRunScore(GameSessionResultDTO result, int score);
        GameOverOptionTypes GetGameOverOptions(GameSessionResultDTO result);
    }

    /// <summary>
    /// Owns the running mode and the services behind it, forwarding every mode-specific call to the
    /// active one. Callers never see a service, so nothing outside branches on the mode.
    /// </summary>
    public class GameModeManager : IGameModeManager
    {
        [Inject] private readonly IList<IGameModeService> _modeServices;

        public GameModeTypes CurrentMode { get; private set; }

        public SceneTypes HubScene => _activeModeService?.HubScene ?? SceneTypes.MainMenu;

        /// <summary>Resolved once per mode change, so no call has to search the list.</summary>
        private IGameModeService _activeModeService;

        public void Initialize()
        {
            InitializeGameMode(CurrentMode);
        }

        /// <summary>A mode with no service is a binding error, so it is logged rather than defaulted over.</summary>
        public void InitializeGameMode(GameModeTypes mode)
        {
            CurrentMode = mode;

            if (!TryGetModeService(mode, out _activeModeService))
            {
                this.LogError($"No game mode service is bound for {mode}.");
            }
        }

        public void ApplyProgressionBonuses(ShipStats stats)
        {
            _activeModeService?.ApplyProgressionBonuses(stats);
        }

        public void SaveLevelResult(GameSessionDTO session, ShipStats stats)
        {
            _activeModeService?.SaveLevelResult(session, stats);
        }

        public void SaveRunScore(GameSessionResultDTO result, int score)
        {
            _activeModeService?.SaveRunScore(result, score);
        }

        public GameOverOptionTypes GetGameOverOptions(GameSessionResultDTO result)
        {
            return _activeModeService?.GetGameOverOptions(result) ?? GameOverOptionTypes.MainMenu;
        }

        private bool TryGetModeService(GameModeTypes mode, out IGameModeService modeService)
        {
            foreach (IGameModeService service in _modeServices)
            {
                if (service.Mode == mode)
                {
                    modeService = service;
                    return true;
                }
            }

            modeService = null;
            return false;
        }
    }
}
