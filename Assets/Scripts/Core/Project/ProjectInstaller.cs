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
            ServicesInstall();
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<MessageBus>().AsSingle();
            Container.BindInterfacesTo<CustomFactory>().AsSingle();
            Container.BindInterfacesTo<ScenesManager>().AsSingle();
            Container.BindInterfacesTo<UIManager>().AsSingle();
            Container.BindInterfacesTo<AddressablesManager>().AsSingle();
            Container.BindInterfacesTo<PersistenceManager>().AsSingle();
            Container.BindInterfacesTo<RepositoryManager>().AsSingle().WithArguments(new object[]
            {
                _configsContainerSO.LevelsDataConfigSO, _configsContainerSO.PlayerDataConfigSO,
                _configsContainerSO.EnemyDataConfigSO, _configsContainerSO.PowerupsDataConfigSO,
                _configsContainerSO.ProjectDataConfigSO, _configsContainerSO.TalentsDataConfigSO,
                _configsContainerSO.SoundsDataConfigSO, _configsContainerSO.ItemsDataConfigSO,
                _configsContainerSO.DropTableConfigSO
            });

            Container.BindInterfacesTo<SoundsManager>().AsSingle();
            Container.BindInterfacesTo<LevelManager>().AsSingle();
            Container.BindInterfacesTo<CurrencyManager>().AsSingle();
            Container.BindInterfacesTo<TalentManager>().AsSingle();
            Container.BindInterfacesTo<InventoryManager>().AsSingle();
            Container.BindInterfacesTo<EquipmentManager>().AsSingle();
        }

        private void ServicesInstall()
        {
            Container.BindInterfacesTo<PlatformService>().AsSingle();
            Container.BindInterfacesTo<SoundsService>().AsSingle();
        }
    }
}