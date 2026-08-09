using System;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    [AddressablePath("Screens/SettingsScreenView")]
    public class SettingsView : View
    {
        [SerializeField] private VolumeSettingsComponent _volumeSettings;
        [SerializeField] private Button _backButton;

        public event Action OnBackClicked;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());

            _volumeSettings.OnMusicVolumeChanged += volume => OnMusicVolumeChanged?.Invoke(volume);
            _volumeSettings.OnSfxVolumeChanged += volume => OnSfxVolumeChanged?.Invoke(volume);
        }

        public void Setup(float musicVolume, float sfxVolume)
        {
            _volumeSettings.Setup(musicVolume, sfxVolume);
        }
    }
}
