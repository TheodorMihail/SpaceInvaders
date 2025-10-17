using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            ManagersInstall();
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<CustomFactory>().AsSingle();
            Container.BindInterfacesTo<ScenesManager>().AsSingle();
            Container.BindInterfacesTo<UIManager>().AsSingle();
            Container.BindInterfacesTo<AddressablesManager>().AsSingle();
            Container.BindInterfacesTo<ErrorManager>().AsSingle();
        }
    }
}