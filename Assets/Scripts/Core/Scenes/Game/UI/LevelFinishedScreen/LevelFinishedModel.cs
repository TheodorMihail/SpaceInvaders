using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class LevelFinishedModel : Model
    {
        [Inject] private readonly ILevelManager _levelManager;

        public bool AllLevelsComplete => _levelManager.CurrentLevelNumber >= _levelManager.MaxLevelNumber;
    }
}
