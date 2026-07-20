using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    public class ProjectInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private ConfigsContainerSO _configsContainerSO;

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
            Container.BindInterfacesTo<PersistenceManager>().AsSingle();
            Container.BindInterfacesTo<ProgressManager>().AsSingle();
            Container.BindInterfacesTo<RepositoryManager>().AsSingle().WithArguments(
                _configsContainerSO.LevelsDataConfigSO, _configsContainerSO.PlayerDataConfigSO,
                _configsContainerSO.EnemyDataConfigSO, _configsContainerSO.PowerupsDataConfigSO);
        }
    }
}