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

            Container.BindInterfacesTo<ProjectRepository>().AsSingle().WithArguments(_configsContainerSO.ProjectDataConfigSO);
            Container.BindInterfacesTo<GameRepository>().AsSingle().WithArguments(_configsContainerSO.GameDataConfigSO);
            Container.BindInterfacesTo<LevelsRepository>().AsSingle().WithArguments(_configsContainerSO.LevelsDataConfigSO);
            Container.BindInterfacesTo<ShipsRepository>().AsSingle().WithArguments(new object[]
            {
                _configsContainerSO.PlayerDataConfigSO, _configsContainerSO.EnemyDataConfigSO
            });
            Container.BindInterfacesTo<PowerupsRepository>().AsSingle().WithArguments(_configsContainerSO.PowerupsDataConfigSO);
            Container.BindInterfacesTo<DropsRepository>().AsSingle().WithArguments(_configsContainerSO.DropTableConfigSO);
            Container.BindInterfacesTo<SoundsRepository>().AsSingle().WithArguments(_configsContainerSO.SoundsDataConfigSO);
            Container.BindInterfacesTo<TalentsRepository>().AsSingle().WithArguments(_configsContainerSO.TalentsDataConfigSO);
            Container.BindInterfacesTo<ItemsRepository>().AsSingle().WithArguments(_configsContainerSO.ItemsDataConfigSO);

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