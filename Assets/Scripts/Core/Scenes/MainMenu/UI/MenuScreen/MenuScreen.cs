using BaseArchitecture.Core;
using static SpaceInvaders.Scenes.MainMenu.MenuScreen;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MenuScreen : Screen<MenuModel, MenuView, MenuController>, IScreenWithResult<MenuScreenResult>
    {
        public enum ResultTypes
        {
            PlayCampaign,
            OpenSettings,
            QuitGame
        }

        public struct MenuScreenResult : IScreenResult
        {
            public ResultTypes Result { get; set; }
        }
        
        private MenuScreenResult _result;

        public MenuScreenResult GetResult()
        {
            return _result;
        }

        public void SetResult(MenuScreenResult result)
        {
            _result = result;
        }
    }
}