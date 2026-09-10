using BaseArchitecture.Core;
using SpaceInvaders.Project;
using System.Collections.Generic;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameEndState;
using static SpaceInvaders.Scenes.Game.GameStateMachine;

namespace SpaceInvaders.Scenes.Game
{
    public class GameStateMachine : BaseStateMachine<GameStateTypes>
    {
        public enum GameStateTypes
        {
            Playing,
            GameEnd
        }

        [Inject] private readonly IScenesManager _scenesManager;
        [Inject] private readonly IGameModeManager _gameModeManager;
        [Inject] private readonly ILevelsRepository _levelsRepository;

        protected override GameStateTypes DefaultStateId => GameStateTypes.Playing;

        private GameSessionDTO _currentSession;

        /// <summary>Kept because the hub is loaded after the result screen, not with it.</summary>
        private GameEndResolutionDTO _currentResolution;

        public GameStateMachine(IList<IState<GameStateTypes>> gameStates) : base(gameStates)
        {
        }

        /// <summary>The launching scene decides the session, so the mode is never assumed here.</summary>
        public override void Initialize()
        {
            _scenesManager.PendingSceneParams.TryGetParam(out _currentSession, CreateCampaignSession(1));
            _gameModeManager.InitializeGameMode(_currentSession.Mode);
            SetState(DefaultStateId, _currentSession);
        }

        protected override void OnStateFinished((GameStateTypes stateId, object[] paramsList) finishedState)
        {
            try
            {
                switch (finishedState.stateId)
                {
                    case GameStateTypes.Playing:
                        GameplayStateResultTypes result = (GameplayStateResultTypes)finishedState.paramsList[0];
                        finishedState.paramsList.TryGetParam(out GameSessionResultDTO sessionResult, default, 1);
                        finishedState.paramsList.TryGetParam(out _currentResolution, default, 2);

                        switch (result)
                        {
                            // Quitting and restarting come from the pause screen, so they skip the
                            // result flow entirely.
                            case GameplayStateResultTypes.Quit:
                                _scenesManager.LoadScene(_gameModeManager.HubScene.ToString(), _currentResolution.HubSceneParams);
                                break;
                            case GameplayStateResultTypes.Restart:
                                _scenesManager.LoadScene(SceneTypes.Game.ToString(), _currentSession);
                                break;
                            default:
                                SetState(GameStateTypes.GameEnd, sessionResult, _currentResolution);
                                break;
                        }

                        break;

                    case GameStateTypes.GameEnd:
                        GameEndStateResultTypes gameEndResult = (GameEndStateResultTypes)finishedState.paramsList[0];
                        switch (gameEndResult)
                        {
                            case GameEndStateResultTypes.ReturnToHub:
                                _scenesManager.LoadScene(_gameModeManager.HubScene.ToString(), _currentResolution.HubSceneParams);
                                break;
                            case GameEndStateResultTypes.Restart:
                                _scenesManager.LoadScene(SceneTypes.Game.ToString(), _currentSession);
                                break;
                            // Re-enters gameplay without reloading the scene, so nothing is disposed
                            // or re-initialized. Per-run state must be reset on game end.
                            // Only Campaign offers Next Level, so advancing by number is safe here.
                            case GameEndStateResultTypes.NextLevel:
                                _currentSession = CreateCampaignSession(_currentSession.LevelNumber + 1);
                                SetState(GameStateTypes.Playing, _currentSession);
                                break;
                        }
                    
                        break;
                }
            }
            catch (System.Exception ex)
            {
                this.LogError($"State transition failed from {finishedState.stateId}", ex);
            }
        }

        /// <summary>Used for the editor-entry default and for advancing a level, both Campaign-only.</summary>
        private GameSessionDTO CreateCampaignSession(int levelNumber)
        {
            return new GameSessionDTO(GameModeTypes.Campaign, levelNumber, _levelsRepository.GetLevelId(levelNumber));
        }
    }
}