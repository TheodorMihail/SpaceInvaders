using BaseArchitecture.Core;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class TimeManagerTests : ZenjectUnitTestFixture
    {
        private static readonly GameSessionDTO _session = new(GameModeTypes.Campaign, 1);
        private static readonly GameSessionResultDTO _sessionResult = new(_session, GameplayStateResultTypes.GameOver);

        private TimeManager _timeManager;
        private IInputManager _mockInputManager;
        private IMessageBus _messageBus;

        private int _pausedCount;
        private int _resumedCount;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _pausedCount = 0;
            _resumedCount = 0;

            _mockInputManager = Substitute.For<IInputManager>();
            _messageBus = new MessageBus();

            Container.Bind<IInputManager>().FromInstance(_mockInputManager);
            Container.Bind<IMessageBus>().FromInstance(_messageBus);

            _timeManager = Container.Instantiate<TimeManager>();
            _timeManager.Initialize();

            _messageBus.Subscribe<GamePausedMessage>(OnGamePaused);
            _messageBus.Subscribe<GameResumedMessage>(OnGameResumed);
        }

        [TearDown]
        public override void Teardown()
        {
            _messageBus.Unsubscribe<GamePausedMessage>(OnGamePaused);
            _messageBus.Unsubscribe<GameResumedMessage>(OnGameResumed);

            _timeManager.Dispose();
            _messageBus.Dispose();

            Time.timeScale = 1f;
            base.Teardown();
        }

        [Test]
        public void Pause_BeforeGameStart_DoesNothing()
        {
            _timeManager.Pause();

            Assert.IsFalse(_timeManager.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(0, _pausedCount);
        }

        [Test]
        public void Pause_AfterGameStart_FreezesTimeAndPublishesMessage()
        {
            _timeManager.GameStart(_session);

            _timeManager.Pause();

            Assert.IsTrue(_timeManager.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void Pause_WhileAlreadyPaused_PublishesOnce()
        {
            _timeManager.GameStart(_session);

            _timeManager.Pause();
            _timeManager.Pause();

            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void Resume_WhilePaused_RestoresTimeAndPublishesMessage()
        {
            _timeManager.GameStart(_session);
            _timeManager.Pause();

            _timeManager.Resume();

            Assert.IsFalse(_timeManager.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(1, _resumedCount);
        }

        [Test]
        public void Resume_WhileNotPaused_DoesNothing()
        {
            _timeManager.GameStart(_session);

            _timeManager.Resume();

            Assert.AreEqual(0, _resumedCount);
        }

        [Test]
        public void GameEnd_WhilePaused_Resumes()
        {
            _timeManager.GameStart(_session);
            _timeManager.Pause();

            _timeManager.GameEnd(_sessionResult);

            Assert.IsFalse(_timeManager.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(1, _resumedCount);
        }

        [Test]
        public void Pause_AfterGameEnd_DoesNothing()
        {
            _timeManager.GameStart(_session);
            _timeManager.GameEnd(_sessionResult);

            _timeManager.Pause();

            Assert.IsFalse(_timeManager.IsPaused);
            Assert.AreEqual(0, _pausedCount);
        }

        [Test]
        public void Pause_OnNextGameStartAfterGameEnd_IsAllowedAgain()
        {
            _timeManager.GameStart(_session);
            _timeManager.GameEnd(_sessionResult);

            _timeManager.GameStart(_session);
            _timeManager.Pause();

            Assert.IsTrue(_timeManager.IsPaused);
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void PauseInput_WhenNotPaused_Pauses()
        {
            _timeManager.GameStart(_session);

            _mockInputManager.OnPause += Raise.Event<System.Action>();

            Assert.IsTrue(_timeManager.IsPaused);
            Assert.AreEqual(1, _pausedCount);
        }

        [Test]
        public void PauseInput_WhilePaused_DoesNotResume()
        {
            _timeManager.GameStart(_session);
            _timeManager.Pause();

            _mockInputManager.OnPause += Raise.Event<System.Action>();

            Assert.IsTrue(_timeManager.IsPaused);
            Assert.AreEqual(0, _resumedCount);
        }

        [Test]
        public void Dispose_RestoresTimeScale()
        {
            _timeManager.GameStart(_session);
            _timeManager.Pause();

            _timeManager.Dispose();

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
