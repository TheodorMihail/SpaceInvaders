using System.Collections.Generic;
using BaseArchitecture.Core;
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
        
        [Header("Configs")]
        [SerializeField] private List<LevelConfigSO> _levelsConfigsSO;
        [SerializeField] private List<PlayerSpaceshipConfigSO> _playerConfigsSO;
        [SerializeField] private List<EnemySpaceshipConfigSO> _enemyConfigsSO;

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
            Container.BindInterfacesTo<InputService>().AsSingle();
            Container.BindInterfacesTo<SpawnService>().AsSingle().WithArguments(_gameContainer);
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<MessageBus>().AsSingle();
            Container.BindInterfacesTo<ObjectPooling>().AsSingle().WithArguments(_objectPoolingContainer);
            Container.BindInterfacesTo<RepositoryManager>().AsSingle().WithArguments(
                _levelsConfigsSO, _playerConfigsSO, _enemyConfigsSO);
                
            Container.BindInterfacesTo<CameraManager>().AsSingle();
            Container.BindInterfacesTo<LevelManager>().AsSingle();
            Container.BindInterfacesTo<PlayerManager>().AsSingle();
            Container.BindInterfacesTo<EnemiesManager>().AsSingle();
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<GameplayState>().AsSingle();
            Container.BindInterfacesTo<GameOverState>().AsSingle();
            Container.BindInterfacesTo<GameStateMachine>().AsSingle();
        }
    }
}