using BaseArchitecture.Core;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            FactoriesInstall();
            ManagersInstall();
        }

        private void FactoriesInstall()
        {
            Container.BindInterfacesTo<Factory>().AsSingle().WithArguments(Container);
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<ScenesManager>().AsSingle();
            Container.BindInterfacesTo<UIManager>().AsSingle();
            Container.BindInterfacesTo<AddressablesManager>().AsSingle();
            Container.BindInterfacesTo<ErrorManager>().AsSingle();
        }
    }
}