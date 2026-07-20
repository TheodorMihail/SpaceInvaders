using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class LevelFinishedModel : Model
    {
        [Inject] private readonly ILevelManager _levelManager;
        [Inject] private readonly IProgressManager _progressManager;

        public bool AllLevelsComplete => _levelManager.CurrentLevelNumber >= _levelManager.MaxLevelNumber;
        public int StarsEarned => _progressManager.GetStars(_levelManager.CurrentLevelNumber);
    }
}
