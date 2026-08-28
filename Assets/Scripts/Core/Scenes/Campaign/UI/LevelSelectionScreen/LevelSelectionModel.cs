using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    public class LevelSelectionModel : Model
    {
        [Inject] private readonly ILevelProgressManager _levelProgressManager;

        public bool IsLevelUnlocked(int levelIndex)
        {
            return _levelProgressManager.IsLevelUnlocked(levelIndex);
        }

        public int GetLevelStars(int levelIndex)
        {
            return _levelProgressManager.GetLevelStars(levelIndex);
        }
    }
}
