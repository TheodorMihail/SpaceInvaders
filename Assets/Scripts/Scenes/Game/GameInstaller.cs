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

        [SerializeField] private PlayerSpaceshipBehaviourComponent _playerPrefab;
        [SerializeField] private LevelConfigSO _levelConfigSO;

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

            Container.TryResolve<ICustomFactory>().UpdateDIContainer(Container);
            Container.TryResolve<IUIManager>().UpdateDIContainer(Container);
        }

        private void ServicesInstall()
        {
            Container.BindInterfacesTo<InputService>().AsSingle();
            Container.BindInterfacesTo<SpawnService>().AsSingle().WithArguments(_gameContainer);
        }

        private void ManagersInstall()
        {
            Container.BindInterfacesTo<LevelManager>().AsSingle().WithArguments(_levelConfigSO);
            Container.BindInterfacesTo<PlayerManager>().AsSingle().WithArguments(_playerPrefab);
            Container.BindInterfacesTo<EnemiesManager>().AsSingle();
            Container.BindInterfacesTo<ObjectPooling>().AsSingle().WithArguments(_objectPoolingContainer);
        }

        private void StateMachineInstall()
        {
            Container.BindInterfacesTo<GameplayState>().AsSingle();
            Container.BindInterfacesTo<GameOverState>().AsSingle();
            Container.BindInterfacesTo<GameStateMachine>().AsSingle();
        }
    }
}