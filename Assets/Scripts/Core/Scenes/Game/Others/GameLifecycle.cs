using System;
using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Scenes.Game
{
    public enum GameplayStateResult
    {
        LevelFinished,
        GameOver
    }

    public interface IGameInitializeListener
    {
        UniTask GameInitialize();
    }

    public interface IGameStartListener
    {
        UniTask GameStart(int levelNumber);
    }

    public interface IGameEndListener
    {
        UniTask GameEnd();
    }

    public interface IGameEndCondition
    {
        event Action<GameplayStateResult> ConditionMet;
    }
}
