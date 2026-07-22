using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IPlatformService
    {
        bool IsTouchPlatform { get; }
        void ApplyFrameRateCap();
    }

    public class PlatformService : IPlatformService
    {
        [Inject] private readonly IRepositoryManager _repositoryManager;

        public bool IsTouchPlatform
        {
            get
            {
#if UNITY_EDITOR
                if (_repositoryManager.GetProjectDataConfig().EditorForceTouchPlatform)
                {
                    return true;
                }
#endif
                return Application.isMobilePlatform;
            }
        }

        public void ApplyFrameRateCap()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _repositoryManager.GetProjectDataConfig().MaxFrameRate;
        }
    }
}
