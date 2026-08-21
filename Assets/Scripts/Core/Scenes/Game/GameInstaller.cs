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
            ServicesInstall();
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

        private void ServicesInstall()
        {
            if (Container.Resolve<IPlatformService>().IsTouchPlatform)
            {
                Container.BindInterfacesTo<TouchInputService>().AsSingle();
            }
            else
            {
                Container.BindInterfacesTo<KeyboardInputService>().AsSingle();
            }

            Container.BindInterfacesTo<PauseService>().AsSingle();
            Container.BindInterfacesTo<SpawnService>().AsSingle().WithArguments(_gameContainer);
            Container.BindInterfacesTo<ScoreService>().AsSingle();
            Container.BindInterfacesTo<LevelSessionService>().AsSingle();
            Container.BindInterfacesTo<HazardsService>().AsSingle();
            Container.BindInterfacesTo<ImpactFeedbackService>().AsSingle();

            Container.BindInterfacesTo<LevelCompletedCondition>().AsSingle();
            Container.BindInterfacesTo<PlayerDestroyedCondition>().AsSingle();

            Container.Bind<IScreenShakeService>().To<ScreenShakeService>().AsSingle().WhenInjectedInto<CameraManager>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.BindInterfacesTo<DebugManager>().AsSingle();
#endif
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<ObjectPooling>().AsSingle().WithArguments(_objectPoolingContainer);
            Container.BindInterfacesTo<CameraManager>().AsSingle();
            Container.BindInterfacesTo<PlayerManager>().AsSingle();
            Container.BindInterfacesTo<EnemiesManager>().AsSingle();
            Container.BindInterfacesTo<PowerupManager>().AsSingle();
            Container.BindInterfacesTo<LootManager>().AsSingle();
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<GameplayState>().AsSingle();
            Container.BindInterfacesTo<GameOverState>().AsSingle();
            Container.BindInterfacesTo<GameStateMachine>().AsSingle();
        }
    }
}