using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IProgressManager : IInitializable
    {
        int GetLevelStars(int levelIndex);
        int LastPlayedLevelStarsEarned { get; }
        bool IsLevelUnlocked(int levelIndex);
        void SetLevelUnlocked(int levelIndex, bool unlocked);
        void RecordLevelResult(int levelIndex, int stars);
    }

    public class ProgressManager : IProgressManager
    {
        private const string SaveKey = "PlayerProgress";

        [Inject] private readonly IPersistenceManager _persistenceManager;
        [Inject] private readonly IRepositoryManager _repositoryManager;

        private GameProgressData _data;

        public int LastPlayedLevelStarsEarned { get; private set; }

        public void Initialize()
        {
            _data = _persistenceManager.Load<GameProgressData>(SaveKey);

            LevelProgressEntry level1 = GetOrCreateLevelProgress(1);
            level1.Unlocked = true;
            SaveProgress();
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
            SaveProgress();
        }

        public void RecordLevelResult(int levelIndex, int stars)
        {
            LastPlayedLevelStarsEarned = stars;

            LevelProgressEntry entry = GetOrCreateLevelProgress(levelIndex);
            if (stars > entry.Stars)
            {
                entry.Stars = stars;
            }

            int nextLevel = levelIndex + 1;
            if (nextLevel <= _repositoryManager.GetLevelsCount())
            {
                GetOrCreateLevelProgress(nextLevel).Unlocked = true;
            }

            SaveProgress();
        }

        private LevelProgressEntry GetLevelProgress(int levelIndex)
        {
            return _data.Levels.Find(l => l.LevelIndex == levelIndex);
        }

        private LevelProgressEntry GetOrCreateLevelProgress(int levelIndex)
        {
            LevelProgressEntry entry = GetLevelProgress(levelIndex);
            if (entry == null)
            {
                entry = new LevelProgressEntry { LevelIndex = levelIndex };
                _data.Levels.Add(entry);
            }

            return entry;
        }

        private void SaveProgress()
        {
            _persistenceManager.Save(SaveKey, _data);
        }
    }
}
