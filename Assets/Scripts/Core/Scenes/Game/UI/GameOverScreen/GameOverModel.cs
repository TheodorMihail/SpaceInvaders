using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverModel : Model
    {
        [Inject] private readonly ILevelSessionManager _levelSessionManager;

        public int TotalScore => _levelSessionManager.TotalScore;
    }
}
