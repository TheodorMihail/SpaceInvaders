using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameOverScreen;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverScreenModel : Model, IModelWithParams<GameOverScreenParams>
    {
        [Inject] private readonly ILevelSessionManager _levelSessionManager;

        public GameEndOptionTypes Options { get; set; } = GameEndOptionTypes.None;

        public int TotalScore => _levelSessionManager.TotalScore;

        public void InitializeWithParameters(GameOverScreenParams parameters)
        {
            Options = parameters.Options;
        }
    }
}
