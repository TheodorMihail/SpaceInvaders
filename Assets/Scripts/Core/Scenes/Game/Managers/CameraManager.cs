using System;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum ScreenRegionTypes
    {
        TopRegion,      // Above the divider (for enemies)
        BottomRegion    // Below the divider (for player)
    }

    public interface ICameraManager : IInitializable, IDisposable
    {
        /// <summary>Where ships may move: inset by the margins reserved for UI.</summary>
        (Vector3 min, Vector3 max) GetPlayableBounds(Renderer renderer, ScreenRegionTypes regionType, float buffer = 0f);

        /// <summary>Where an object is still on screen: the full view, ignoring the UI margins.</summary>
        (Vector3 min, Vector3 max) GetVisibleBounds(Renderer renderer, float buffer = 0f);

        Vector3 GetViewportWorldPoint(float viewportX, float viewportY, float yPosition);
        Vector3 GetScreenPoint(Vector3 worldPosition);
    }

    public class CameraManager : ICameraManager
    {
        [Inject] private readonly IGameRepository _gameRepository;

        private Camera _mainCamera;
        private GameDataConfigSO _gameDataConfig;

        public void Initialize()
        {
            _mainCamera = Camera.main;
            _gameDataConfig = _gameRepository.GetGameDataConfig();

            if (_mainCamera == null)
            {
                Debug.LogError("CameraManager: No main camera found!");
            }
        }

        public void Dispose()
        {
            _mainCamera = null;
            _gameDataConfig = null;
        }

        /// <summary>
        /// Returns the world-space movement bounds for a renderer within the given screen region,
        /// inset by the renderer's extents, the buffer, and the margins reserved for UI.
        /// </summary>
        public (Vector3 min, Vector3 max) GetPlayableBounds(Renderer renderer, ScreenRegionTypes regionType, float buffer = 0f)
        {
            if (_mainCamera == null || renderer == null || _gameDataConfig == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            Vector3 position = renderer.transform.position;
            Vector3 extents = renderer.bounds.extents;

            float sideMargin = _gameDataConfig.SideMarginRatio;
            Vector3 screenBottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(sideMargin, _gameDataConfig.BottomMarginRatio, position.y));
            Vector3 screenTopRight = _mainCamera.ViewportToWorldPoint(new Vector3(1f - sideMargin, 1f - _gameDataConfig.TopMarginRatio, position.y));
            Vector3 screenDivider = _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, _gameDataConfig.RegionDividerRatio, position.y));

            Vector3 minBounds;
            Vector3 maxBounds;

            switch (regionType)
            {
                case ScreenRegionTypes.TopRegion:
                    // Above the divider, up to the top margin
                    minBounds = new Vector3(
                        screenBottomLeft.x + extents.x + buffer,
                        position.y,
                        screenDivider.z + extents.z + buffer
                    );
                    maxBounds = new Vector3(
                        screenTopRight.x - extents.x - buffer,
                        position.y,
                        screenTopRight.z - extents.z - buffer
                    );
                    break;

                case ScreenRegionTypes.BottomRegion:
                default:
                    // From the bottom margin up to the divider
                    minBounds = new Vector3(
                        screenBottomLeft.x + extents.x + buffer,
                        position.y,
                        screenBottomLeft.z + extents.z + buffer
                    );
                    maxBounds = new Vector3(
                        screenTopRight.x - extents.x - buffer,
                        position.y,
                        screenDivider.z - extents.z - buffer
                    );
                    break;

            }

            // If the renderer's own extents are large relative to the region (e.g. a big boss),
            // insetting by the full extents on both sides can invert min/max, which makes
            // Mathf.Clamp snap to an edge instead of bouncing smoothly. Collapse to the midpoint instead.
            if (minBounds.x > maxBounds.x)
            {
                float midX = (minBounds.x + maxBounds.x) * 0.5f;
                minBounds.x = midX;
                maxBounds.x = midX;
            }

            if (minBounds.z > maxBounds.z)
            {
                float midZ = (minBounds.z + maxBounds.z) * 0.5f;
                minBounds.z = midZ;
                maxBounds.z = midZ;
            }

            return (minBounds, maxBounds);
        }

        /// <summary>
        /// The full view, expanded by the renderer's extents so an object is only considered gone
        /// once it has fully left the screen. Deliberately ignores the UI margins, which restrict
        /// where ships may move rather than what is still visible.
        /// </summary>
        public (Vector3 min, Vector3 max) GetVisibleBounds(Renderer renderer, float buffer = 0f)
        {
            if (_mainCamera == null || renderer == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            Vector3 position = renderer.transform.position;
            Vector3 extents = renderer.bounds.extents;

            Vector3 bottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, position.y));
            Vector3 topRight = _mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, position.y));

            Vector3 minBounds = new Vector3(
                bottomLeft.x - extents.x - buffer,
                position.y,
                bottomLeft.z - extents.z - buffer
            );
            Vector3 maxBounds = new Vector3(
                topRight.x + extents.x + buffer,
                position.y,
                topRight.z + extents.z + buffer
            );

            return (minBounds, maxBounds);
        }

        public Vector3 GetViewportWorldPoint(float viewportX, float viewportY, float yPosition)
        {
            if (_mainCamera == null)
            {
                return Vector3.zero;
            }

            return _mainCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, yPosition));
        }

        /// <summary>Projects a world position to screen pixels, for placing UI over gameplay.</summary>
        public Vector3 GetScreenPoint(Vector3 worldPosition)
        {
            if (_mainCamera == null)
            {
                return Vector3.zero;
            }

            return _mainCamera.WorldToScreenPoint(worldPosition);
        }
    }
}
