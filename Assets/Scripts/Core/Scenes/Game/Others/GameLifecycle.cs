using System;
using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Scenes.Game
{
    public enum GameModeTypes
    {
        Campaign
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
        public int LevelNumber { get; }

        public GameSessionDTO(GameModeTypes mode, int levelNumber)
        {
            Mode = mode;
            LevelNumber = levelNumber;
        }
    }

    /// <summary>The session plus why it ended, so listeners do not have to track the outcome themselves.</summary>
    public readonly struct GameSessionResultDTO
    {
        public GameSessionDTO Session { get; }
        public GameplayStateResultTypes Result { get; }

        public GameSessionResultDTO(GameSessionDTO session, GameplayStateResultTypes result)
        {
            Session = session;
            Result = result;
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
