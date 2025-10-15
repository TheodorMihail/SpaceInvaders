using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class MainMenuInstaller : MonoInstaller
    {
        public Transform ScreensContainer;

        public override void InstallBindings()
        {
            ContainersInstall();
            StateMachineInstall();
        }

        private void ContainersInstall()
        {
            Container.Bind<Transform>().WithId(IScreen.ScreensContainerID)
                .FromInstance(ScreensContainer).AsSingle();

            Container.Resolve<IUIManager>().UpdateDIContainer(Container);
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<MenuState>().AsSingle();
            Container.BindInterfacesTo<MainMenuStateMachine>().AsSingle();
        }
    }
}