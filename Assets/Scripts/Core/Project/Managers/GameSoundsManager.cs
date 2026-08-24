using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Scenes.Game;
using SpaceInvaders.Scenes.MainMenu;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IGameSoundsManager : ISoundsManager
    {
        void PlaySound(SoundTypes type);
        float GetCategoryVolume(SoundCategoryTypes category);
        void SetCategoryVolume(SoundCategoryTypes category, float volume);
    }

    /// <summary>Resolves authored sounds against the generic playback system and owns the music per
    /// lifecycle phase.</summary>
    public class GameSoundsManager : SoundsManager, IGameSoundsManager, IDisposable,
        IGameInitializeListener, IGameEndListener, IMenuEnterListener
    {
        [Inject] private readonly ISoundsRepository _soundsRepository;
        [Inject] private readonly ISoundsService _soundsService;

        /// <summary>Explicit, so the kernel reaches this rather than the non-virtual base.</summary>
        void IInitializable.Initialize()
        {
            base.Initialize();

            _soundsService.OnSoundRequested += PlaySound;
            _soundsService.Initialize();
        }

        public void Dispose()
        {
            _soundsService.OnSoundRequested -= PlaySound;
            _soundsService.Dispose();
        }

        public UniTask MenuEnter()
        {
            PlaySound(SoundTypes.MenuMusic);
            return UniTask.CompletedTask;
        }

        public UniTask GameInitialize()
        {
            PlaySound(SoundTypes.GameplayMusic);
            return UniTask.CompletedTask;
        }

        public UniTask GameEnd()
        {
            PlaySound(SoundTypes.GameOver);
            return UniTask.CompletedTask;
        }

        public void PlaySound(SoundTypes type)
        {
            if (!_soundsRepository.TryGetSoundConfig(type, out SoundConfigSO config))
            {
                return;
            }

            string channel = config.Category.ToString();

            if (config.Loop)
            {
                PlayLoop(config.Clip, channel, config.Volume);
            }
            else
            {
                PlayOneShot(config.Clip, channel, config.Volume);
            }
        }

        public float GetCategoryVolume(SoundCategoryTypes category)
        {
            return GetChannelVolume(category.ToString());
        }

        public void SetCategoryVolume(SoundCategoryTypes category, float volume)
        {
            SetChannelVolume(category.ToString(), volume);
        }
    }
}
