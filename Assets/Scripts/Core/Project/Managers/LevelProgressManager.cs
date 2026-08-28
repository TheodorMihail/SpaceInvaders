using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface ILevelProgressManager : IInitializable
    {
        int MaxLevelNumber { get; }

        int GetLevelStars(int levelIndex);
        int LastPlayedLevelStarsEarned { get; }
        bool IsLevelUnlocked(int levelIndex);
        void SetLevelUnlocked(int levelIndex, bool unlocked);
        void RecordLevelResult(int levelIndex, int stars);
    }

    public partial class LevelProgressManager : ILevelProgressManager
    {
        [Inject] private readonly ISaveProfileManager _saveProfileManager;
        [Inject] private readonly ILevelsRepository _levelsRepository;

        private IPersistenceManager _persistenceManager;
        private LevelsSaveData _data;

        public int MaxLevelNumber => _levelsRepository.GetLevelsCount();
        public int LastPlayedLevelStarsEarned { get; private set; }

        /// <summary>Level progress is Campaign's alone, so this profile never varies.</summary>
        public void Initialize()
        {
            _persistenceManager = _saveProfileManager.GetProfile(GameModeTypes.Campaign);
            _data = _persistenceManager.LoadVersioned<LevelsSaveData>(LevelsSaveData.SaveKey, LevelsSaveData.CurrentVersion);

            LevelSaveEntry firstLevel = GetOrCreateLevelProgress(1);
            firstLevel.Unlocked = true;
            SaveData();
        }

        public int GetLevelStars(int levelIndex)
        {
            return GetLevelProgress(levelIndex)?.Stars ?? 0;
        }

        /// <summary>Falls back to the previous level's completion, so levels added after a save was
        /// written still unlock without replaying.</summary>
        public bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex <= 1)
            {
                return true;
            }

            if (GetLevelProgress(levelIndex)?.Unlocked ?? false)
            {
                return true;
            }

            return GetLevelStars(levelIndex - 1) > 0;
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
