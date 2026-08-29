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
            RepositoriesInstall();
            ManagersInstall();
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<MessageBus>().AsSingle();
            Container.BindInterfacesTo<CustomFactory>().AsSingle();
            
            Container.BindInterfacesTo<ScenesManager>().AsSingle();
            Container.BindInterfacesTo<UIManager>().AsSingle();
            Container.BindInterfacesTo<AddressablesManager>().AsSingle();
            Container.BindInterfacesTo<SaveProfileManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<LevelProgressManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<CurrencyManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<TalentManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EquipmentManager>().AsSingle();

            Container.BindInterfacesTo<PlatformManager>()
                .FromSubContainerResolve().ByInstaller<PlatformInstaller>().AsSingle();
            Container.BindInterfacesTo<GameSoundsManager>()
                .FromSubContainerResolve().ByInstaller<SoundsInstaller>().AsSingle();
            Container.BindInterfacesAndSelfTo<InventoryManager>()
                .FromSubContainerResolve().ByInstaller<InventoryInstaller>().AsSingle();
            Container.BindInterfacesTo<GameModeManager>()
                .FromSubContainerResolve().ByInstaller<GameModeInstaller>().AsSingle();
            Container.BindInterfacesAndSelfTo<ExpeditionRunManager>()
                .FromSubContainerResolve().ByInstaller<ExpeditionRunInstaller>().AsSingle();
        }

        private void RepositoriesInstall()
        {
            Container.BindInterfacesTo<ProjectRepository>().AsSingle().WithArguments(_configsContainerSO.ProjectDataConfigSO);
            Container.BindInterfacesTo<GameRepository>().AsSingle().WithArguments(_configsContainerSO.GameDataConfigSO);
            Container.BindInterfacesTo<LevelsRepository>().AsSingle().WithArguments(_configsContainerSO.LevelsDataConfigSO);
            Container.BindInterfacesTo<PowerupsRepository>().AsSingle().WithArguments(_configsContainerSO.PowerupsDataConfigSO);
            Container.BindInterfacesTo<DropsRepository>().AsSingle().WithArguments(_configsContainerSO.DropTableConfigSO);
            Container.BindInterfacesTo<SoundsRepository>().AsSingle().WithArguments(_configsContainerSO.SoundsDataConfigSO);
            Container.BindInterfacesTo<TalentsRepository>().AsSingle().WithArguments(_configsContainerSO.TalentsDataConfigSO);
            Container.BindInterfacesTo<ItemsRepository>().AsSingle().WithArguments(_configsContainerSO.ItemsDataConfigSO);
            Container.BindInterfacesTo<HazardsRepository>().AsSingle().WithArguments(_configsContainerSO.HazardsDataConfigSO);
            Container.BindInterfacesTo<ExpeditionRepository>().AsSingle().WithArguments(_configsContainerSO.ExpeditionDataConfigSO);
            Container.BindInterfacesTo<ShipsRepository>().AsSingle().WithArguments(new object[]
           {
                _configsContainerSO.PlayerDataConfigSO, _configsContainerSO.EnemyDataConfigSO
           });
        }
    }

    public class PlatformInstaller : Installer<PlatformInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<PlatformManager>().AsSingle();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.Bind<IScreenshotService>().To<ScreenshotService>().AsSingle();
#endif
        }
    }

    public class SoundsInstaller : Installer<SoundsInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<GameSoundsManager>().AsSingle();
            Container.Bind<ISoundsService>().To<SoundsService>().AsSingle();
        }
    }

    public class GameModeInstaller : Installer<GameModeInstaller>
    {
        public override void InstallBindings()
        {
            // Concrete: the parent's subcontainer lookup asks for this type, not the interfaces.
            Container.Bind<GameModeManager>().AsSingle();
            Container.Bind<IGameModeService>().To<CampaignModeService>().AsSingle();
            Container.Bind<IGameModeService>().To<ExpeditionModeService>().AsSingle();
        }
    }

    public class ExpeditionRunInstaller : Installer<ExpeditionRunInstaller>
    {
        public override void InstallBindings()
        {
            // Concrete: the parent's subcontainer lookup asks for this type, not the interfaces.
            Container.Bind<ExpeditionRunManager>().AsSingle();
            Container.Bind<IExpeditionMapService>().To<ExpeditionMapService>().AsSingle();
        }
    }

    public class InventoryInstaller : Installer<InventoryInstaller>
    {
        public override void InstallBindings()
        {
            // Concrete: the parent's subcontainer lookup asks for this type, not the interfaces.
            Container.Bind<InventoryManager>().AsSingle();
            Container.Bind<IItemStorageService>().To<ItemStorageService>().AsSingle();
            Container.Bind<IItemSellService>().To<ItemSellService>().AsSingle();
        }
    }
}