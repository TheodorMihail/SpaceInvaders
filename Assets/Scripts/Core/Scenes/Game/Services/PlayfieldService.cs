using SpaceInvaders.Project;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public interface IPlayfieldService
    {
        /// <summary>The play area in viewport space. One of its two extents is always the full 1.</summary>
        Rect ViewportRect { get; }

        /// <summary>Zooms the camera out until the whole play area fits, and updates the rect.</summary>
        void SetupToPlayfield(Camera camera);

        /// <summary>Maps a point inside the play area, from 0 to 1 on each axis, into viewport space.</summary>
        Vector2 ToViewportPoint(float playfieldX, float playfieldY);
    }

    /// <summary>
    /// Holds the authored play area and sizes the camera to it, so the arena covers the same world
    /// units and the same shape whatever the display's aspect.
    /// </summary>
    public class PlayfieldService : IPlayfieldService
    {
        private readonly float _referenceAspectRatio;
        private readonly float _referenceOrthographicSize;

        public Rect ViewportRect { get; private set; } = new Rect(0f, 0f, 1f, 1f);

        public PlayfieldService(IGameRepository gameRepository)
        {
            GameDataConfigSO gameDataConfig = gameRepository.GetGameDataConfig();

            _referenceAspectRatio = gameDataConfig.ReferenceAspectRatio;
            _referenceOrthographicSize = gameDataConfig.ReferenceOrthographicSize;
        }

        public void SetupToPlayfield(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            float orthographicSize = GetPlayfieldOrthographicSize(_referenceOrthographicSize, _referenceAspectRatio, camera.aspect);

            camera.orthographicSize = orthographicSize;
            ViewportRect = GetViewportRect(_referenceOrthographicSize, _referenceAspectRatio, camera.aspect, orthographicSize);
        }

        public Vector2 ToViewportPoint(float playfieldX, float playfieldY)
        {
            Rect rect = ViewportRect;

            return new Vector2(rect.x + playfieldX * rect.width, rect.y + playfieldY * rect.height);
        }

        /// <summary>Sized so the whole area fits rather than so the screen is filled, which would crop it.</summary>
        public static float GetPlayfieldOrthographicSize(float referenceOrthographicSize, float referenceAspectRatio, float screenAspectRatio)
        {
            if (screenAspectRatio <= 0f)
            {
                return referenceOrthographicSize;
            }

            return Mathf.Max(referenceOrthographicSize, referenceOrthographicSize * referenceAspectRatio / screenAspectRatio);
        }

        public static Rect GetViewportRect(float referenceOrthographicSize, float referenceAspectRatio, float screenAspectRatio, float playfieldOrthographicSize)
        {
            if (screenAspectRatio <= 0f || playfieldOrthographicSize <= 0f)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            // Nothing is ever cropped, so clamped only against rounding.
            float width = Mathf.Min(referenceOrthographicSize * referenceAspectRatio / (playfieldOrthographicSize * screenAspectRatio), 1f);
            float height = Mathf.Min(referenceOrthographicSize / playfieldOrthographicSize, 1f);

            return new Rect((1f - width) * 0.5f, (1f - height) * 0.5f, width, height);
        }
    }
}
