using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameOverModel : Model
    {
        [Inject] private readonly IScoreService _scoreService;

        public int TotalScore => _scoreService.TotalScore;
    }
}
