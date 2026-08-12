using System;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public enum ScreenRegionTypes
    {
        Full,           // Entire screen
        TopHalf,        // Top half (for enemies)
        BottomHalf      // Bottom half (for player)
    }

    public interface ICameraManager : IInitializable, IDisposable
    {
        (Vector3 min, Vector3 max) GetScreenBounds(Renderer renderer, ScreenRegionTypes regionType, float buffer = 0f);
        Vector3 GetViewportWorldPoint(float viewportX, float viewportY, float yPosition);
        Vector3 GetScreenPoint(Vector3 worldPosition);
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

        /// <summary>
        /// Returns the world-space movement bounds for a renderer within the given screen region,
        /// inset by the renderer's extents. The buffer shrinks the half regions and expands the full one.
        /// </summary>
        public (Vector3 min, Vector3 max) GetScreenBounds(Renderer renderer, ScreenRegionTypes regionType, float buffer = 0f)
        {
            if (_mainCamera == null || renderer == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            Vector3 position = renderer.transform.position;
            Vector3 extents = renderer.bounds.extents;

            Vector3 screenBottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0.03f, position.y));
            Vector3 screenTopRight = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, position.y));
            Vector3 screenCenter = _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, position.y));

            Vector3 minBounds;
            Vector3 maxBounds;

            switch (regionType)
            {
                case ScreenRegionTypes.TopHalf:
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

                case ScreenRegionTypes.BottomHalf:
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

                case ScreenRegionTypes.Full:
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
