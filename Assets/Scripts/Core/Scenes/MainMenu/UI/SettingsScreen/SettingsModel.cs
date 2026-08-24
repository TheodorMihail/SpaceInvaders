using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class SettingsModel : Model
    {
        [Inject] private readonly IGameSoundsManager _gameSoundsManager;

        public float MusicVolume => _gameSoundsManager.GetCategoryVolume(SoundCategoryTypes.Music);
        public float SfxVolume => _gameSoundsManager.GetCategoryVolume(SoundCategoryTypes.SFX);

        public void SetMusicVolume(float volume)
        {
            _gameSoundsManager.SetCategoryVolume(SoundCategoryTypes.Music, volume);
        }

        public void SetSfxVolume(float volume)
        {
            _gameSoundsManager.SetCategoryVolume(SoundCategoryTypes.SFX, volume);
        }
    }
}
