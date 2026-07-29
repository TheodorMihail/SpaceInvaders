using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class LevelSelectionModel : Model
    {
        [Inject] private readonly ILevelManager _levelManager;

        public bool IsLevelUnlocked(int levelIndex)
        {
            return _levelManager.IsLevelUnlocked(levelIndex);
        }

        public int GetLevelStars(int levelIndex)
        {
            return _levelManager.GetLevelStars(levelIndex);
        }
    }
}
