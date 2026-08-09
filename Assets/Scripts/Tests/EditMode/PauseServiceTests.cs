using BaseArchitecture.Core;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class PauseServiceTests : ZenjectUnitTestFixture
    {
        private PauseService _pauseService;
        private IInputService _mockInputService;
        private IMessageBus _messageBus;

        private int _pausedCount;
        private int _resumedCount;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _pausedCount = 0;
            _resumedCount = 0;

            _mockInputService = Substitute.For<IInputService>();
            _messageBus = new MessageBus();

            Container.Bind<IInputService>().FromInstance(_mockInputService);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);

            _pauseService = Container.Instantiate<PauseService>();
            _pauseService.Initialize();

            _messageBus.Subscribe<GamePausedMessage>(OnGamePaused);
            _messageBus.Subscribe<GameResumedMessage>(OnGameResumed);
        }

        [TearDown]
        public override void Teardown()
        {
            _messageBus.Unsubscribe<GamePausedMessage>(OnGamePaused);
            _messageBus.Unsubscribe<GameResumedMessage>(OnGameResumed);

            _pauseService.Dispose();
            _messageBus.Dispose();

            Time.timeScale = 1f;
            base.Teardown();
        }

        [Test]
        public void Pause_BeforeGameStart_DoesNothing()
        {
            _pauseService.Pause();

            Assert.IsFalse(_pauseService.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(0, _pausedCount);
        }

        [Test]
        public void Pause_AfterGameStart_FreezesTimeAndPublishesMessage()
        {
            _pauseService.GameStart(1);

            _pauseService.Pause();

            Assert.IsTrue(_pauseService.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void Pause_WhileAlreadyPaused_PublishesOnce()
        {
            _pauseService.GameStart(1);

            _pauseService.Pause();
            _pauseService.Pause();

            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void Resume_WhilePaused_RestoresTimeAndPublishesMessage()
        {
            _pauseService.GameStart(1);
            _pauseService.Pause();

            _pauseService.Resume();

            Assert.IsFalse(_pauseService.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(1, _resumedCount);
        }

        [Test]
        public void Resume_WhileNotPaused_DoesNothing()
        {
            _pauseService.GameStart(1);

            _pauseService.Resume();

            Assert.AreEqual(0, _resumedCount);
        }

        [Test]
        public void GameEnd_WhilePaused_Resumes()
        {
            _pauseService.GameStart(1);
            _pauseService.Pause();

            _pauseService.GameEnd();

            Assert.IsFalse(_pauseService.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(1, _resumedCount);
        }

        [Test]
        public void Pause_AfterGameEnd_DoesNothing()
        {
            _pauseService.GameStart(1);
            _pauseService.GameEnd();

            _pauseService.Pause();

            Assert.IsFalse(_pauseService.IsPaused);
            Assert.AreEqual(0, _pausedCount);
        }

        [Test]
        public void Pause_OnNextGameStartAfterGameEnd_IsAllowedAgain()
        {
            _pauseService.GameStart(1);
            _pauseService.GameEnd();

            _pauseService.GameStart(2);
            _pauseService.Pause();

            Assert.IsTrue(_pauseService.IsPaused);
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void PauseInput_WhenNotPaused_Pauses()
        {
            _pauseService.GameStart(1);

            _mockInputService.OnPause += Raise.Event<System.Action>();

            Assert.IsTrue(_pauseService.IsPaused);
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void PauseInput_WhilePaused_DoesNotResume()
        {
            _pauseService.GameStart(1);
            _pauseService.Pause();

            _mockInputService.OnPause += Raise.Event<System.Action>();

            Assert.IsTrue(_pauseService.IsPaused);
            Assert.AreEqual(0, _resumedCount);
        }

        [Test]
        public void Dispose_RestoresTimeScale()
        {
            _pauseService.GameStart(1);
            _pauseService.Pause();

            _pauseService.Dispose();

            Assert.AreEqual(1f, Time.timeScale);
        }

        private void OnGamePaused(GamePausedMessage message)
        {
            _pausedCount++;
        }

        private void OnGameResumed(GameResumedMessage message)
        {
            _resumedCount++;
        }
    }
}
