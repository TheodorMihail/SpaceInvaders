using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private Transform _screensContainer;
        [SerializeField] private Transform _hudContainer;
        [SerializeField] private Transform _gameContainer;
        [SerializeField] private Transform _objectPoolingContainer;

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

            Container.Resolve<ICustomFactory>().UpdateDIContainer(Container);
            Container.Resolve<IUIManager>().UpdateDIContainer(Container);
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<ObjectPooling>().AsSingle().WithArguments(_objectPoolingContainer);
            Container.BindInterfacesTo<SpawnManager>().AsSingle().WithArguments(_gameContainer);
            Container.BindInterfacesTo<PlayerManager>().AsSingle();
            Container.BindInterfacesTo<PowerupManager>().AsSingle();

            Container.BindInterfacesTo<InputManager>()
                .FromSubContainerResolve().ByInstaller<InputInstaller>().AsSingle();
            Container.BindInterfacesTo<TimeManager>().AsSingle();
            Container.BindInterfacesTo<CameraManager>()
                .FromSubContainerResolve().ByInstaller<CameraInstaller>().AsSingle();
            Container.BindInterfacesTo<LevelSessionManager>()
                .FromSubContainerResolve().ByInstaller<LevelSessionInstaller>().AsSingle();
            Container.BindInterfacesTo<LootManager>()
                .FromSubContainerResolve().ByInstaller<LootInstaller>().AsSingle();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.BindInterfacesTo<DebugManager>().AsSingle();
#endif
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<GameplayState>()
                .FromSubContainerResolve().ByInstaller<GameplayStateInstaller>().AsSingle();

            Container.BindInterfacesTo<GameOverState>().AsSingle();
            Container.BindInterfacesTo<GameStateMachine>().AsSingle();
        }
    }

    public class InputInstaller : Installer<InputInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<InputManager>().AsSingle();

            if (Container.Resolve<IPlatformManager>().IsTouchPlatform)
            {
                Container.Bind<IInputService>().To<TouchInputService>().AsSingle();
            }
            else
            {
                Container.Bind<IInputService>().To<KeyboardInputService>().AsSingle();
            }
        }
    }

    public class CameraInstaller : Installer<CameraInstaller>
    {
        public override void InstallBindings()
        {
            // Concrete: the parent's subcontainer lookup asks for this type, not the interfaces.
            Container.Bind<CameraManager>().AsSingle();
            Container.Bind<IScreenShakeService>().To<ScreenShakeService>().AsSingle();
        }
    }

    public class LevelSessionInstaller : Installer<LevelSessionInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<LevelSessionManager>().AsSingle();
            Container.Bind<IEnemiesService>().To<EnemiesService>().AsSingle();
            Container.Bind<IHazardsService>().To<HazardsService>().AsSingle();
            Container.Bind<IScoreService>().To<ScoreService>().AsSingle();
            Container.Bind<IImpactFeedbackService>().To<ImpactFeedbackService>().AsSingle();
        }
    }

    public class LootInstaller : Installer<LootInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<LootManager>().AsSingle();
            Container.Bind<IDropRollService>().To<DropRollService>().AsSingle();
        }
    }

    public class GameplayStateInstaller : Installer<GameplayStateInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<GameplayState>().AsSingle();

            Container.Bind<IGameEndCondition>().To<LevelCompletedCondition>().AsSingle();
            Container.Bind<IGameEndCondition>().To<PlayerDestroyedCondition>().AsSingle();
        }
    }
}