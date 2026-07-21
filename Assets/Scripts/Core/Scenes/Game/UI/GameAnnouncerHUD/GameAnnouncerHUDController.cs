using System.Collections.Generic;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GameAnnouncerHUDController : Controller<GameAnnouncerHUD, GameAnnouncerHUDModel, GameAnnouncerHUDView>
    {
        [Inject] private readonly IMessageBus _messageBus;

        private readonly Queue<string> _pendingAnnouncements = new();
        private bool _isPlaying;
        private bool _isDisposed;

        public GameAnnouncerHUDController(GameAnnouncerHUD hud, GameAnnouncerHUDModel model, GameAnnouncerHUDView view)
            : base(hud, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _messageBus.Subscribe<LevelStartedMessage>(OnLevelStarted);
            _messageBus.Subscribe<WaveStartedMessage>(OnWaveStarted);
            _messageBus.Subscribe<GameEndedMessage>(OnGameEnded);
        }

        public override void Dispose()
        {
            base.Dispose();

            _isDisposed = true;
            _pendingAnnouncements.Clear();

            _messageBus.Unsubscribe<LevelStartedMessage>(OnLevelStarted);
            _messageBus.Unsubscribe<WaveStartedMessage>(OnWaveStarted);
            _messageBus.Unsubscribe<GameEndedMessage>(OnGameEnded);
        }

        private void OnLevelStarted(LevelStartedMessage message)
        {
            EnqueueAnnouncement(message.LevelName);
        }

        private void OnWaveStarted(WaveStartedMessage message)
        {
            string announcementText = message.IsBossWave ? _model.BossWaveText : _model.NormalWaveTextFormat(message.WaveNumber);
            EnqueueAnnouncement(announcementText);
        }

        private void OnGameEnded(GameEndedMessage message)
        {
            Close();
        }

        private void EnqueueAnnouncement(string text)
        {
            _pendingAnnouncements.Enqueue(text);

            if (!_isPlaying)
            {
                PlayPendingAnnouncements().Forget();
            }
        }

        private async UniTaskVoid PlayPendingAnnouncements()
        {
            _isPlaying = true;

            while (!_isDisposed && _pendingAnnouncements.Count > 0)
            {
                string text = _pendingAnnouncements.Dequeue();
                _view.SetDisplayText(text);
                await _view.PlayAnimation(_model.AnimationDurationSeconds);
            }

            _isPlaying = false;
        }
    }
}
