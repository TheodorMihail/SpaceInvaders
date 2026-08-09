using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Project
{
    /// <summary>Music and SFX volume sliders, shared by every screen that exposes audio settings.
    /// Sliders are stepped rather than continuous, since each change is persisted.</summary>
    public class VolumeSettingsComponent : MonoBehaviour
    {
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;

        private void Awake()
        {
            _musicSlider.onValueChanged.AddListener(value => OnMusicVolumeChanged?.Invoke(ToVolume(_musicSlider, value)));
            _sfxSlider.onValueChanged.AddListener(value => OnSfxVolumeChanged?.Invoke(ToVolume(_sfxSlider, value)));
        }

        public void Setup(float musicVolume, float sfxVolume)
        {
            _musicSlider.SetValueWithoutNotify(ToSliderValue(_musicSlider, musicVolume));
            _sfxSlider.SetValueWithoutNotify(ToSliderValue(_sfxSlider, sfxVolume));
        }

        private static float ToVolume(Slider slider, float sliderValue)
        {
            return Mathf.InverseLerp(slider.minValue, slider.maxValue, sliderValue);
        }

        private static float ToSliderValue(Slider slider, float volume)
        {
            return Mathf.Lerp(slider.minValue, slider.maxValue, volume);
        }
    }
}
