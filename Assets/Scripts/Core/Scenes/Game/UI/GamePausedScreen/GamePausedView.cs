using System;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Shown while the time scale is zero, so nothing here may animate on scaled time.</summary>
    [AddressablePath("Screens/GamePausedScreenView")]
    public class GamePausedView : View
    {
        [SerializeField] private VolumeSettingsComponent _volumeSettings;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        public event Action OnResumeButtonClicked;
        public event Action OnRestartButtonClicked;
        public event Action OnQuitButtonClicked;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;

        private void Awake()
        {
            _resumeButton.onClick.AddListener(() => OnResumeButtonClicked?.Invoke());
            _restartButton.onClick.AddListener(() => OnRestartButtonClicked?.Invoke());
            _quitButton.onClick.AddListener(() => OnQuitButtonClicked?.Invoke());

            _volumeSettings.OnMusicVolumeChanged += volume => OnMusicVolumeChanged?.Invoke(volume);
            _volumeSettings.OnSfxVolumeChanged += volume => OnSfxVolumeChanged?.Invoke(volume);
        }

        public void Setup(float musicVolume, float sfxVolume)
        {
            _volumeSettings.Setup(musicVolume, sfxVolume);
        }
    }
}
