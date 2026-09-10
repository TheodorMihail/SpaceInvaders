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
        DropTableTypes DropTableType { get; }
        bool CanReplayLevel { get; }

        /// <summary>Called once on entering a mode's hub or the Game scene. The mode never changes
        /// within a run.</summary>
        void InitializeGameMode(GameModeTypes mode);

        void ApplyProgressionBonuses(ShipStats stats);
        GameEndResolutionDTO ResolveGameEnd(GameSessionResultDTO result);
    }

    /// <summary>A manager whose whole state lives in the running mode's save profile, so it reloads on
    /// every mode change instead of loading once at boot.</summary>
    public interface IGameModeScopedManager
    {
        void LoadForMode(GameModeTypes mode);

        /// <summary>Wipes the loaded mode's store, for a mode whose progression lasts one run.</summary>
        void ClearLoadedData();
    }

    /// <summary>
    /// Owns the running mode: the rules behind it, which every mode-specific call is forwarded to, and
    /// the managers scoped to it. Callers never see a mode's rules, so nothing outside branches on it.
    /// </summary>
    public class GameModeManager : IGameModeManager
    {
        [Inject] private readonly IList<IGameModeRules> _modeRules;
        [Inject] private readonly IList<IGameModeScopedManager> _modeScopedManagers;

        public GameModeTypes CurrentMode { get; private set; }

        public SceneTypes HubScene => _activeRules?.HubScene ?? SceneTypes.MainMenu;
        public DropTableTypes DropTableType => _activeRules?.DropTableType ?? DropTableTypes.Campaign;
        public bool CanReplayLevel => _activeRules?.CanReplayLevel ?? true;

        /// <summary>Resolved once per mode change, so no call has to search the list.</summary>
        private IGameModeRules _activeRules;

        public void Initialize()
        {
            InitializeGameMode(CurrentMode);
        }

        /// <summary>A mode with no rules is a binding error, so it is logged rather than defaulted over.</summary>
        public void InitializeGameMode(GameModeTypes mode)
        {
            CurrentMode = mode;

            if (!TryGetModeRules(mode, out _activeRules))
            {
                this.LogError($"No game mode rules are bound for {mode}.");
            }

            // Before anything reads them, so a hub screen never draws the previous mode's progression.
            foreach (IGameModeScopedManager modeScopedManager in _modeScopedManagers)
            {
                modeScopedManager.LoadForMode(mode);
            }
        }

        public void ApplyProgressionBonuses(ShipStats stats)
        {
            _activeRules?.ApplyProgressionBonuses(stats);
        }

        public GameEndResolutionDTO ResolveGameEnd(GameSessionResultDTO result)
        {
            if (_activeRules == null)
            {
                return new GameEndResolutionDTO(GameEndOptionTypes.MainMenu);
            }

            return _activeRules.ResolveGameEnd(result);
        }

        private bool TryGetModeRules(GameModeTypes mode, out IGameModeRules rules)
        {
            foreach (IGameModeRules modeRules in _modeRules)
            {
                if (modeRules.Mode == mode)
                {
                    rules = modeRules;
                    return true;
                }
            }

            rules = null;
            return false;
        }
    }
}
