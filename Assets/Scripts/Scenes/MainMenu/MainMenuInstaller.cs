using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MainMenuInstaller : MonoInstaller
    {
        [SerializeField] private Transform _screensContainer;

        public override void InstallBindings()
        {
            ContainersInstall();
            StateMachineInstall();
        }

        private void ContainersInstall()
        {
            Container.Bind<Transform>().WithId(IScreen.ScreensContainerID)
                .FromInstance(_screensContainer).AsCached();

            Container.TryResolve<IUIManager>().UpdateDIContainer(Container);
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<MenuState>().AsSingle();
            Container.BindInterfacesTo<MainMenuStateMachine>().AsSingle();
        }
    }
}