using System;
using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Scenes.Game
{
    public enum GameModeTypes
    {
        Campaign,
        Expedition
    }

    public enum GameplayStateResultTypes
    {
        LevelFinished,
        GameOver,
        Restart,
        Quit
    }

    /// <summary>The run being played: which mode launched it and which level it plays.</summary>
    public readonly struct GameSessionDTO
    {
        public GameModeTypes Mode { get; }

        /// <summary>Campaign: the level. Expedition: the node's depth. Display and progress only.</summary>
        public int LevelNumber { get; }

        /// <summary>Which level to load, whatever the mode used to choose it.</summary>
        public string LevelId { get; }

        public GameSessionDTO(GameModeTypes mode, int levelNumber, string levelId)
        {
            Mode = mode;
            LevelNumber = levelNumber;
            LevelId = levelId;
        }
    }

    /// <summary>Everything about how a level ended, so nothing has to be gathered a second time.</summary>
    public readonly struct GameSessionResultDTO
    {
        public GameSessionDTO Session { get; }
        public GameplayStateResultTypes Result { get; }
        public int Score { get; }

        /// <summary>The ship as it finished. Null when it did not survive.</summary>
        public ShipStats Stats { get; }

        public GameSessionResultDTO(GameSessionDTO session, GameplayStateResultTypes result, int score = 0,
            ShipStats stats = null)
        {
            Session = session;
            Result = result;
            Score = score;
            Stats = stats;
        }
    }

    /// <summary>Game initialization, before any UI is shown.</summary>
    public interface IGameInitializeListener
    {
        UniTask GameInitialize(GameSessionDTO session);
    }

    /// <summary>Game start, after the start screen is dismissed.</summary>
    public interface IGameStartListener
    {
        UniTask GameStart(GameSessionDTO session);
    }

    /// <summary>Game end. Per-run state is reset here rather than on dispose, since advancing to the
    /// next level does not dispose anything.</summary>
    public interface IGameEndListener
    {
        UniTask GameEnd(GameSessionResultDTO result);
    }

    public interface IGameEndCondition
    {
        event Action<GameplayStateResultTypes> ConditionMet;

        void GameStart(GameSessionDTO session);
        void GameEnd();
    }
}
