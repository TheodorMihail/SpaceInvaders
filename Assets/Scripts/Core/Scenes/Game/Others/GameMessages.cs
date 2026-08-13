using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public readonly struct GameEndedMessage : IMessageObject
    {
    }

    public readonly struct GamePausedMessage : IMessageObject
    {
    }

    public readonly struct GameResumedMessage : IMessageObject
    {
    }

    public readonly struct EnemyDestroyedMessage : IMessageObject
    {
        public EnemyTypes Type { get; }
        public EnemyCategoryTypes Category { get; }
        public Vector3 LocalPosition { get; }

        public EnemyDestroyedMessage(EnemyTypes type, EnemyCategoryTypes category, Vector3 localPosition)
        {
            Type = type;
            Category = category;
            LocalPosition = localPosition;
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
        public int TotalWaves { get; }
        public bool IsBossWave { get; }

        public WaveStartedMessage(int waveNumber, int totalWaves, bool isBossWave)
        {
            WaveNumber = waveNumber;
            TotalWaves = totalWaves;
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

    public readonly struct PlayerAmmoChangedMessage : IMessageObject
    {
        public int CurrentAmmo { get; }
        public int MaxAmmo { get; }

        public PlayerAmmoChangedMessage(int currentAmmo, int maxAmmo)
        {
            CurrentAmmo = currentAmmo;
            MaxAmmo = maxAmmo;
        }
    }

    /// <summary>Reload completion arrives as a full PlayerAmmoChangedMessage, so there is no paired end message.</summary>
    public readonly struct PlayerReloadStartedMessage : IMessageObject
    {
        public float Duration { get; }

        public PlayerReloadStartedMessage(float duration)
        {
            Duration = duration;
        }
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

    public readonly struct PowerupDroppedMessage : IMessageObject
    {
        public PowerupTypes Type { get; }
        public Vector3 LocalPosition { get; }

        public PowerupDroppedMessage(PowerupTypes type, Vector3 localPosition)
        {
            Type = type;
            LocalPosition = localPosition;
        }
    }

    public readonly struct ItemDroppedMessage : IMessageObject
    {
        public string InstanceId { get; }
        public Vector3 LocalPosition { get; }

        public ItemDroppedMessage(string instanceId, Vector3 localPosition)
        {
            InstanceId = instanceId;
            LocalPosition = localPosition;
        }
    }

    public readonly struct ItemCollectedMessage : IMessageObject
    {
        public string InstanceId { get; }
        public ItemRarityTypes Rarity { get; }

        public ItemCollectedMessage(string instanceId, ItemRarityTypes rarity)
        {
            InstanceId = instanceId;
            Rarity = rarity;
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

    public readonly struct ButtonClickedMessage : IMessageObject
    {
    }

    public readonly struct ShipShotFiredMessage : IMessageObject
    {
        public Vector3 LocalPosition { get; }

        public ShipShotFiredMessage(Vector3 localPosition)
        {
            LocalPosition = localPosition;
        }
    }

    public readonly struct ShipDamagedMessage : IMessageObject
    {
        public int CurrentHealth { get; }
        public int Damage { get; }
        public bool IsCritical { get; }
        public Vector3 WorldPosition { get; }

        public ShipDamagedMessage(int currentHealth, int damage, bool isCritical, Vector3 worldPosition)
        {
            CurrentHealth = currentHealth;
            Damage = damage;
            IsCritical = isCritical;
            WorldPosition = worldPosition;
        }
    }
}
