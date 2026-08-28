using Cysharp.Threading.Tasks;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Entering a non-gameplay scene. One interface rather than one per scene, so a scene added later
    /// needs no new lifecycle type.
    /// </summary>
    public interface ISceneEnterListener
    {
        UniTask SceneEnter(SceneTypes scene);
    }
}
