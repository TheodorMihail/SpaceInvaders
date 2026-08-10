using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class GamePausedModel : Model
    {
        [Inject] private readonly ISoundsService _soundsService;

        public float MusicVolume => _soundsService.GetCategoryVolume(SoundCategoryTypes.Music);
        public float SfxVolume => _soundsService.GetCategoryVolume(SoundCategoryTypes.SFX);

        public void SetMusicVolume(float volume)
        {
            _soundsService.SetCategoryVolume(SoundCategoryTypes.Music, volume);
        }

        public void SetSfxVolume(float volume)
        {
            _soundsService.SetCategoryVolume(SoundCategoryTypes.SFX, volume);
        }
    }
}
