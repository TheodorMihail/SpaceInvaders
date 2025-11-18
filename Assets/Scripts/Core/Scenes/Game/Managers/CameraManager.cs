using System;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum ScreenRegionType
    {
        Full,           // Entire screen
        TopHalf,        // Top half (for enemies)
        BottomHalf      // Bottom half (for player)
    }

    public interface ICameraManager : IInitializable, IDisposable
    {
        (Vector3 min, Vector3 max) GetScreenBounds(Renderer renderer, ScreenRegionType regionType, float buffer = 0f);
        Vector3 GetViewportWorldPoint(float viewportX, float viewportY, float yPosition);
    }

    public class CameraManager : ICameraManager
    {
        private Camera _mainCamera;

        public void Initialize()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("CameraManager: No main camera found!");
            }
        }

        public void Dispose()
        {
            _mainCamera = null;
        }

        public (Vector3 min, Vector3 max) GetScreenBounds(Renderer renderer, ScreenRegionType regionType, float buffer = 0f)
        {
            if (_mainCamera == null || renderer == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            Vector3 position = renderer.transform.position;
            Vector3 extents = renderer.bounds.extents;

            // Calculate screen positions
            Vector3 screenBottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, position.y));
            Vector3 screenTopRight = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, position.y));
            Vector3 screenCenter = _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, position.y));

            Vector3 minBounds;
            Vector3 maxBounds;

            switch (regionType)
            {
                case ScreenRegionType.TopHalf:
                    // Upper half of screen (center to top)
                    minBounds = new Vector3(
                        screenBottomLeft.x + extents.x + buffer,
                        position.y,
                        screenCenter.z + extents.z + buffer
                    );
                    maxBounds = new Vector3(
                        screenTopRight.x - extents.x - buffer,
                        position.y,
                        screenTopRight.z - extents.z - buffer
                    );
                    break;

                case ScreenRegionType.BottomHalf:
                    // Lower half of screen (bottom to center)
                    minBounds = new Vector3(
                        screenBottomLeft.x + extents.x + buffer,
                        position.y,
                        screenBottomLeft.z + extents.z + buffer
                    );
                    maxBounds = new Vector3(
                        screenTopRight.x - extents.x - buffer,
                        position.y,
                        screenCenter.z - extents.z - buffer
                    );
                    break;

                case ScreenRegionType.Full:
                default:
                    // Full screen with buffer extending beyond edges
                    minBounds = new Vector3(
                        screenBottomLeft.x - buffer,
                        position.y,
                        screenBottomLeft.z - buffer
                    );
                    maxBounds = new Vector3(
                        screenTopRight.x + buffer,
                        position.y,
                        screenTopRight.z + buffer
                    );
                    break;
            }

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
    }
}
