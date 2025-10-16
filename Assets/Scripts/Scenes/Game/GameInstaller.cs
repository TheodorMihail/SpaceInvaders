using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private Transform _screensContainer;
        [SerializeField] private Transform _hudContainer;

        public override void InstallBindings()
        {
            ContainersInstall();
            ManagersInstall();
            StateMachineInstall();
        }

        private void ContainersInstall()
        {
            Container.Bind<Transform>().WithId(IScreen.ScreensContainerID)
                .FromInstance(_screensContainer).AsCached();
            Container.Bind<Transform>().WithId(IHUD.HUDContainerID)
                .FromInstance(_hudContainer).AsCached();

            Container.TryResolve<IUIManager>().UpdateDIContainer(Container);
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<GameplayManager>().AsSingle();
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<GameplayState>().AsSingle();
            Container.BindInterfacesTo<GameOverState>().AsSingle();
            Container.BindInterfacesTo<GameStateMachine>().AsSingle();
        }
    }
}