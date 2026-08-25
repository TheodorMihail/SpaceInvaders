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

    public interface ICameraManager : IInitializable, IDisposable, ITickable
    {
        /// <summary>Adds to the shake the camera is already carrying. It decays on its own, so callers
        /// never have to stop one.</summary>
        void AddScreenShake(float amount);

        /// <summary>Drops all shake and puts the camera back on its mark.</summary>
        void ResetShake();

        /// <summary>Where ships may move: inset by the margins reserved for UI.</summary>
        (Vector3 min, Vector3 max) GetPlayableBounds(Renderer renderer, ScreenRegionTypes regionType, float buffer = 0f);

        /// <summary>Where an object is still on screen: the full view, ignoring the UI margins.</summary>
        (Vector3 min, Vector3 max) GetVisibleBounds(Renderer renderer, float buffer = 0f);

        /// <summary>Where something entering from above comes in: the top edge of the view on the
        /// given **world** plane, spanning left to right. Takes a plane rather than a renderer, since
        /// the object being placed does not exist yet. The inset is a fraction of the view's width
        /// taken off each side, for callers that must not leave it hanging half off screen.</summary>
        (Vector3 left, Vector3 right) GetTopEdgeBounds(float planeY, float sideInsetRatio = 0f);

        /// <summary>How much of the renderer has come into view from above, from 0 fully hidden to 1
        /// fully shown, for anything that should react to arriving rather than to being spawned.</summary>
        float GetVisibleFraction(Renderer renderer);

        Vector3 GetViewportWorldPoint(float viewportX, float viewportY, float yPosition);
        Vector3 GetScreenPoint(Vector3 worldPosition);
    }

    public class CameraManager : ICameraManager
    {
        [Inject] private readonly IGameRepository _gameRepository;

        /// <summary>Reachable from nowhere else, so the camera keeps a single owner while the shake
        /// maths lives on its own.</summary>
        [Inject] private readonly IScreenShakeService _screenShake;

        private Camera _mainCamera;
        private GameDataConfigSO _gameDataConfig;

        /// <summary>Where the camera sits when it is not being shaken. Bounds are always measured
        /// from here, never from the shaken pose.</summary>
        private Vector3 _restPosition;

        /// <summary>How far the camera currently sits from its mark, which every bounds query has to
        /// take back out so it answers from the resting pose.</summary>
        private Vector3 ShakeOffset => _screenShake.Offset;

        public void Initialize()
        {
            _mainCamera = Camera.main;
            _gameDataConfig = _gameRepository.GetGameDataConfig();

            if (_mainCamera == null)
            {
                Debug.LogError("CameraManager: No main camera found!");
                return;
            }

            _restPosition = _mainCamera.transform.position;
        }

        public void Dispose()
        {
            ResetShake();
            _mainCamera = null;
            _gameDataConfig = null;
        }

        public void AddScreenShake(float amount)
        {
            _screenShake.Add(amount);
        }

        public void ResetShake()
        {
            _screenShake.Reset();
            ApplyShakeOffset();
        }

        /// <summary>Scaled time on purpose: the shake freezes with the rest of the game while paused
        /// or held in slow motion, rather than rattling on over a still frame.</summary>
        public void Tick()
        {
            _screenShake.Tick(Time.deltaTime);
            ApplyShakeOffset();
        }

        private void ApplyShakeOffset()
        {
            if (_mainCamera == null)
            {
                return;
            }

            _mainCamera.transform.position = _restPosition + ShakeOffset;
        }

        /// <summary>
        /// Returns the world-space movement bounds for a renderer within the given screen region,
        /// inset by the renderer's extents, the buffer, and the margins reserved for UI.
        /// </summary>
        /// <remarks>Answers from the camera's resting pose. Callers cache these on first use, so a
        /// query landing mid-shake would otherwise bake the shake into a ship's playfield for life.</remarks>
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

            return (minBounds - ShakeOffset, maxBounds - ShakeOffset);
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

            return (minBounds - ShakeOffset, maxBounds - ShakeOffset);
        }

        /// <summary>
        /// The top edge of the full view, so anything placed along it slides in rather than appearing
        /// mid-screen. Deliberately ignores the UI margins for the same reason the visible bounds do:
        /// they restrict where ships may move, not where the view begins.
        /// </summary>
        public (Vector3 left, Vector3 right) GetTopEdgeBounds(float planeY, float sideInsetRatio = 0f)
        {
            if (_mainCamera == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            // Half the width per side is the whole width, so anything beyond that would invert the span.
            float inset = Mathf.Clamp(sideInsetRatio, 0f, 0.5f);

            Vector3 leftEdge = _mainCamera.ViewportToWorldPoint(new Vector3(inset, 1f, planeY));
            Vector3 rightEdge = _mainCamera.ViewportToWorldPoint(new Vector3(1f - inset, 1f, planeY));

            return (
                new Vector3(leftEdge.x, planeY, leftEdge.z) - ShakeOffset,
                new Vector3(rightEdge.x, planeY, rightEdge.z) - ShakeOffset
            );
        }

        /// <summary>
        /// The visible bounds already carry the renderer's extents, so their maximum is the point where
        /// it is exactly fully hidden above the view, and two extents further in is where it is exactly
        /// fully shown. The fraction is where it sits between the two.
        /// </summary>
        public float GetVisibleFraction(Renderer renderer)
        {
            if (_mainCamera == null || renderer == null)
            {
                return 0f;
            }

            float extentZ = renderer.bounds.extents.z;

            if (extentZ <= 0f)
            {
                return 0f;
            }

            (Vector3 _, Vector3 maxBounds) = GetVisibleBounds(renderer);
            float fullyShownZ = maxBounds.z - extentZ * 2f;

            return Mathf.InverseLerp(maxBounds.z, fullyShownZ, renderer.transform.position.z);
        }

        public Vector3 GetViewportWorldPoint(float viewportX, float viewportY, float yPosition)
        {
            if (_mainCamera == null)
            {
                return Vector3.zero;
            }

            return _mainCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, yPosition)) - ShakeOffset;
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
