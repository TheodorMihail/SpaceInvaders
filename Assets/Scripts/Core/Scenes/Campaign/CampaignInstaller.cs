using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    public class CampaignInstaller : MonoInstaller
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
            Container.BindInterfacesTo<DebugManager>().AsSingle();
#endif
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<CampaignHubState>().AsSingle();
            Container.BindInterfacesTo<CampaignStateMachine>().AsSingle();
        }
    }
}
