using System;
using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPauseService : IGameStartListener, IGameEndListener
    {
        bool IsPaused { get; }
        void Pause();
        void Resume();

        /// <summary>Runs time down for a moment so a beat lands. Pause always wins: pausing cancels
        /// the slow motion, and one already in flight never restores time behind the pause screen.</summary>
        void ApplySlowMotion(float duration, float timeScale);
    }

    /// <summary>Owns the paused flag and the time scale for a run. Pausing is only possible between
    /// game start and game end.</summary>
    public class PauseService : IPauseService, IInitializable, IDisposable
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IInputService _inputService;

        private bool _canPause;
        private CancellationTokenSource _slowMotionCancellationTokenSource;

        public bool IsPaused { get; private set; }

        public void Initialize()
        {
            _inputService.OnPause += OnPauseInput;
            Application.focusChanged += OnApplicationFocusChanged;
        }

        public void Dispose()
        {
            _inputService.OnPause -= OnPauseInput;
            Application.focusChanged -= OnApplicationFocusChanged;

            CancelSlowMotion();
            Time.timeScale = 1f;
        }

        public UniTask GameStart(int levelNumber)
        {
            _canPause = true;
            return UniTask.CompletedTask;
        }

        /// <summary>Restores time outright rather than through Resume, which only acts when paused.
        /// Slow motion still in flight would otherwise carry into the next level, since the level
        /// advance never reloads the scene.</summary>
        public UniTask GameEnd()
        {
            Resume();
            CancelSlowMotion();
            Time.timeScale = 1f;
            _canPause = false;
            return UniTask.CompletedTask;
        }

        public void Pause()
        {
            if (!_canPause || IsPaused)
            {
                return;
            }

            // Slow motion in flight must not restore time from behind the pause screen.
            CancelSlowMotion();

            IsPaused = true;
            Time.timeScale = 0f;
            _messageBus.Publish(new GamePausedMessage());
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            _messageBus.Publish(new GameResumedMessage());
        }

        public void ApplySlowMotion(float duration, float timeScale)
        {
            if (!_canPause || IsPaused || duration <= 0f)
            {
                return;
            }

            CancelSlowMotion();

            _slowMotionCancellationTokenSource = new CancellationTokenSource();
            RunSlowMotion(duration, Mathf.Clamp01(timeScale), _slowMotionCancellationTokenSource.Token).Forget();
        }

        /// <summary>The wait is unscaled, or the slowdown would stretch the very delay timing it.</summary>
        private async UniTaskVoid RunSlowMotion(float duration, float timeScale, CancellationToken token)
        {
            Time.timeScale = timeScale;

            await UniTask.Delay(TimeSpan.FromSeconds(duration), DelayType.UnscaledDeltaTime, cancellationToken: token);

            if (token.IsCancellationRequested || IsPaused)
            {
                return;
            }

            Time.timeScale = 1f;
        }

        private void CancelSlowMotion()
        {
            _slowMotionCancellationTokenSource?.CancelAndDispose();
            _slowMotionCancellationTokenSource = null;
        }

        /// <summary>Resuming from input is owned by the pause screen while it is open.</summary>
        private void OnPauseInput()
        {
            if (IsPaused)
            {
                return;
            }

            Pause();
        }

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                return;
            }

            Pause();
        }
    }
}
