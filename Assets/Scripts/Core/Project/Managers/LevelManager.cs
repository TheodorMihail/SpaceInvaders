using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ILevelManager : IInitializable
    {
        int MaxLevelNumber { get; }
        int CurrentLevelNumber { get; }

        int GetLevelStars(int levelIndex);
        int LastPlayedLevelStarsEarned { get; }
        bool IsLevelUnlocked(int levelIndex);
        void SetLevelUnlocked(int levelIndex, bool unlocked);
        void RecordLevelResult(int levelIndex, int stars);

        void RegisterSession(ILevelSessionService session);
        void UnregisterSession(ILevelSessionService session);
    }

    public partial class LevelManager : ILevelManager
    {
        [Inject] private readonly IPersistenceManager _persistenceManager;
        [Inject] private readonly ILevelsRepository _levelsRepository;

        private LevelsSaveData _data;
        private ILevelSessionService _activeSession;

        public int MaxLevelNumber => _levelsRepository.GetLevelsCount();
        public int CurrentLevelNumber => _activeSession?.CurrentLevelNumber ?? 0;
        public int LastPlayedLevelStarsEarned { get; private set; }

        public void Initialize()
        {
            _data = _persistenceManager.Load<LevelsSaveData>(LevelsSaveData.SaveKey);

            LevelSaveEntry firstLevel = GetOrCreateLevelProgress(1);
            firstLevel.Unlocked = true;
            SaveData();
        }

        public int GetLevelStars(int levelIndex)
        {
            return GetLevelProgress(levelIndex)?.Stars ?? 0;
        }

        public bool IsLevelUnlocked(int levelIndex)
        {
            return GetLevelProgress(levelIndex)?.Unlocked ?? false;
        }

        public void SetLevelUnlocked(int levelIndex, bool unlocked)
        {
            GetOrCreateLevelProgress(levelIndex).Unlocked = unlocked;
            SaveData();
        }

        /// <summary>Stores the star count if it improves the previous result, and unlocks the next level.</summary>
        public void RecordLevelResult(int levelIndex, int stars)
        {
            LastPlayedLevelStarsEarned = stars;

            LevelSaveEntry entry = GetOrCreateLevelProgress(levelIndex);
            if (stars > entry.Stars)
            {
                entry.Stars = stars;
            }

            int nextLevel = levelIndex + 1;
            if (nextLevel <= _levelsRepository.GetLevelsCount())
            {
                GetOrCreateLevelProgress(nextLevel).Unlocked = true;
            }

            SaveData();
        }

        /// <summary>Registers the active gameplay session. CurrentLevelNumber returns 0 while none
        /// is registered.</summary>
        public void RegisterSession(ILevelSessionService session)
        {
            _activeSession = session;
        }

        public void UnregisterSession(ILevelSessionService session)
        {
            if (_activeSession == session)
            {
                _activeSession = null;
            }
        }

        private LevelSaveEntry GetLevelProgress(int levelIndex)
        {
            return _data.Levels.Find(l => l.LevelIndex == levelIndex);
        }

        private LevelSaveEntry GetOrCreateLevelProgress(int levelIndex)
        {
            LevelSaveEntry entry = GetLevelProgress(levelIndex);
            if (entry == null)
            {
                entry = new LevelSaveEntry { LevelIndex = levelIndex };
                _data.Levels.Add(entry);
            }

            return entry;
        }

        private void SaveData()
        {
            _persistenceManager.Save(LevelsSaveData.SaveKey, _data);
        }
    }
}
