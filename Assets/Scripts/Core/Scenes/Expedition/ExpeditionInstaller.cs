using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Expedition
{
    public class ExpeditionInstaller : MonoInstaller
    {
        [SerializeField] private Transform _screensContainer;

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

            Container.Resolve<ICustomFactory>().UpdateDIContainer(Container);
            Container.Resolve<IUIManager>().UpdateDIContainer(Container);
        }

        private void ManagersInstall()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.BindInterfacesTo<ExpeditionDebugCommands>().AsSingle();
            Container.BindInterfacesTo<DebugManager>().AsSingle();
#endif
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<ExpeditionHubState>().AsSingle();
            Container.BindInterfacesTo<ExpeditionMapState>().AsSingle();
            Container.BindInterfacesTo<ExpeditionStateMachine>().AsSingle();
        }
    }
}
