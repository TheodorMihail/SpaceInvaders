using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    public interface IPlatformManager
    {
        bool IsTouchPlatform { get; }
        void ApplyFrameRateCap();
    }

    /// <summary>What the device this build runs on can do, and the app-level settings that follow.</summary>
    public partial class PlatformManager : IPlatformManager
    {
        [Inject] private readonly IProjectRepository _projectRepository;

        public bool IsTouchPlatform => GetIsTouchPlatform();

        private bool GetIsTouchPlatform()
        {
#if UNITY_EDITOR
            if (_projectRepository.GetProjectDataConfig().EditorForceTouchPlatform)
            {
                return true;
            }
#endif
            return Application.isMobilePlatform;
        }

        public void ApplyFrameRateCap()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = _projectRepository.GetProjectDataConfig().MaxFrameRate;
        }
    }
}
