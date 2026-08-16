#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>Captures the game view straight to disk. Reaching for an external capture tool costs
    /// the application its focus, which auto pauses the run and puts the pause screen in the shot.</summary>
    public class ScreenshotService : IDebugCommandProvider
    {
        private const string FileNameTimeFormat = "yyyy-MM-dd_HH-mm-ss";

        public IReadOnlyList<DebugCommandDTO> GetDebugCommands()
        {
            return new[]
            {
                new DebugCommandDTO(DebugKeys.TakeScreenshot, "Take screenshot", TakeScreenshot)
            };
        }

        private void TakeScreenshot()
        {
            // Named after the game, since the file lands loose among everything else on the desktop.
            string fileName = $"{Application.productName}_{DateTime.Now.ToString(FileNameTimeFormat)}.png";
            string filePath = Path.Combine(GetDesktopPath(), fileName);

            // Captured at the end of the frame, so the file lands shortly after this returns.
            ScreenCapture.CaptureScreenshot(filePath);
            this.LogWarning($"Debug: Screenshot saved to {filePath}");
        }

        /// <summary>Falls back to the save data folder on platforms that have no desktop.</summary>
        private string GetDesktopPath()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            if (string.IsNullOrEmpty(desktopPath))
            {
                return Application.persistentDataPath;
            }

            return desktopPath;
        }
    }
}
#endif
