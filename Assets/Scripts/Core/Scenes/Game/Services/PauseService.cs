using System;
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
    }

    /// <summary>Owns the paused flag and the time scale for a run. Pausing is only possible between
    /// game start and game end.</summary>
    public class PauseService : IPauseService, IInitializable, IDisposable
    {
        [Inject] private readonly IMessageBus _messageBus;
        [Inject] private readonly IInputService _inputService;

        private bool _canPause;

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

            Time.timeScale = 1f;
        }

        public UniTask GameStart(int levelNumber)
        {
            _canPause = true;
            return UniTask.CompletedTask;
        }

        public UniTask GameEnd()
        {
            Resume();
            _canPause = false;
            return UniTask.CompletedTask;
        }

        public void Pause()
        {
            if (!_canPause || IsPaused)
            {
                return;
            }

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
