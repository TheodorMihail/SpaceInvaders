using System;
using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Scenes.Game
{
    public enum GameplayStateResultTypes
    {
        LevelFinished,
        GameOver,
        Restart,
        Quit
    }

    /// <summary>Game initialization, before any UI is shown.</summary>
    public interface IGameInitializeListener
    {
        UniTask GameInitialize();
    }

    /// <summary>Game start, after the start screen is dismissed.</summary>
    public interface IGameStartListener
    {
        UniTask GameStart(int levelNumber);
    }

    /// <summary>Game end. Per-run state is reset here rather than on dispose, since advancing to the
    /// next level does not dispose anything.</summary>
    public interface IGameEndListener
    {
        UniTask GameEnd();
    }

    /// <summary>Condition that ends the game when met.</summary>
    public interface IGameEndCondition
    {
        event Action<GameplayStateResultTypes> ConditionMet;

        void GameStart();
        void GameEnd();
    }
}
