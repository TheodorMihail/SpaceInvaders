using System;
using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Scenes.Game
{
    public enum GameplayStateResultTypes
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
        event Action<GameplayStateResultTypes> ConditionMet;
    }
}
