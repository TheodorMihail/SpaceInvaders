using System;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using SpaceInvaders.Scenes.Game;
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
        IGameInitializeListener, IGameEndListener, ISceneEnterListener
    {
        [Inject] private readonly ISoundsRepository _soundsRepository;
        [Inject] private readonly ISoundsService _soundsService;
        [Inject] private readonly ISaveProfileManager _saveProfileManager;

        private IPersistenceManager _persistenceManager;

        /// <summary>Volume settings belong to no mode, so they live in the general profile.</summary>
        public override void Initialize()
        {
            _persistenceManager = _saveProfileManager.GetGeneralProfile();
            InitializeWithSettings(_persistenceManager.Load<SoundsSaveData>(SoundsSaveData.SaveKey));

            _soundsService.OnSoundRequested += PlaySound;
            _soundsService.Initialize();
        }

        protected override void SaveSettings(SoundsSaveData settings)
        {
            _persistenceManager.Save(SoundsSaveData.SaveKey, settings);
        }

        public void Dispose()
        {
            _soundsService.OnSoundRequested -= PlaySound;
            _soundsService.Dispose();
        }

        /// <summary>Every non-gameplay scene shares the menu music, so the scene is not read yet.</summary>
        public UniTask SceneEnter(SceneTypes scene)
        {
            PlaySound(SoundTypes.MenuMusic);
            return UniTask.CompletedTask;
        }

        public UniTask GameInitialize(GameSessionDTO session)
        {
            PlaySound(SoundTypes.GameplayMusic);
            return UniTask.CompletedTask;
        }

        /// <summary>Only a defeat gets the sting. Clearing the level, restarting and quitting do not.</summary>
        public UniTask GameEnd(GameSessionResultDTO result)
        {
            if (result.Result == GameplayStateResultTypes.GameOver)
            {
                PlaySound(SoundTypes.GameOver);
            }

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
