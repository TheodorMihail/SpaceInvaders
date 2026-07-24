using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Scenes.MainMenu
{
    public interface IMenuEnterListener
    {
        UniTask MenuEnter();
    }
}
