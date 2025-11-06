using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.MainMenu.MenuScreen;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MenuScreen : Screen<MenuModel, MenuView, MenuController>, IScreenWithResult<MenuScreenResult>
    {
        public enum ResultType
        {
            PlayGame,
            QuitGame
        }

        public struct MenuScreenResult : IScreenResult
        {
            public ResultType Result { get; set; }
        }
        
        private MenuScreenResult _result;
        public MenuScreenResult GetResult() => _result;
        public void SetResult(MenuScreenResult result) => _result = result;
    }
}