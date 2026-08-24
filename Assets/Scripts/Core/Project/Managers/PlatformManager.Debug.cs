#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using BaseArchitecture.Core;
using Zenject;

namespace SpaceInvaders.Project
{
    public partial class PlatformManager : IDebugCommandProvider
    {
        [Inject] private readonly IScreenshotService _screenshotService;

        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.TakeScreenshot, "Take screenshot", DebugTakeScreenshot)
            };
        }

        private void DebugTakeScreenshot()
        {
            _screenshotService.TakeScreenshot();
        }
    }
}
#endif
