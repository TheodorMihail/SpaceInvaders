using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public readonly struct GameEndedMessage : IMessageObject
    {
    }

    public readonly struct EnemyDestroyedMessage : IMessageObject
    {
        public EnemyTypes Type { get; }
        public EnemyCategory Category { get; }
        public Vector3 Position { get; }

        public EnemyDestroyedMessage(EnemyTypes type, EnemyCategory category, Vector3 position)
        {
            Type = type;
            Category = category;
            Position = position;
        }
    }

    public readonly struct AllEnemiesDestroyedMessage : IMessageObject
    {
    }

    public readonly struct BossSpawnedMessage : IMessageObject
    {
        public EnemyTypes Type { get; }
        public int MaxHealth { get; }

        public BossSpawnedMessage(EnemyTypes type, int maxHealth)
        {
            Type = type;
            MaxHealth = maxHealth;
        }
    }

    public readonly struct LevelStartedMessage : IMessageObject
    {
        public int LevelNumber { get; }
        public string LevelName { get; }

        public LevelStartedMessage(int levelNumber, string levelName)
        {
            LevelNumber = levelNumber;
            LevelName = levelName;
        }
    }

    public readonly struct WaveStartedMessage : IMessageObject
    {
        public int WaveNumber { get; }
        public bool IsBossWave { get; }

        public WaveStartedMessage(int waveNumber, bool isBossWave)
        {
            WaveNumber = waveNumber;
            IsBossWave = isBossWave;
        }
    }

    public readonly struct BossHealthChangedMessage : IMessageObject
    {
        public int CurrentHealth { get; }
        public int MaxHealth { get; }

        public BossHealthChangedMessage(int currentHealth, int maxHealth)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    public readonly struct LevelCompletedMessage : IMessageObject
    {
        public int LevelNumber { get; }

        public LevelCompletedMessage(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }

    public readonly struct PlayerDestroyedMessage : IMessageObject
    {
    }

    public readonly struct PowerupActivatedMessage : IMessageObject
    {
        public PowerupTypes Type { get; }
        public float Duration { get; }

        public PowerupActivatedMessage(PowerupTypes type, float duration)
        {
            Type = type;
            Duration = duration;
        }
    }

    public readonly struct PowerupExpiredMessage : IMessageObject
    {
        public PowerupTypes Type { get; }

        public PowerupExpiredMessage(PowerupTypes type)
        {
            Type = type;
        }
    }

    public readonly struct ScoreChangedMessage : IMessageObject
    {
        public int TotalScore { get; }
        public int Delta { get; }

        public ScoreChangedMessage(int totalScore, int delta)
        {
            TotalScore = totalScore;
            Delta = delta;
        }
    }
}
