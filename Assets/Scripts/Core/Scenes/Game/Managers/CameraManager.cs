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
        /// <summary>The play area in viewport space, for UI that should follow the arena rather than
        /// the screen edges.</summary>
        Rect PlayfieldViewportRect { get; }

        /// <summary>Adds to the shake the camera is already carrying. It decays on its own, so callers
        /// never have to stop one.</summary>
        void AddScreenShake(float amount);

        /// <summary>Drops all shake and puts the camera back on its mark.</summary>
        void ResetShake();

        /// <summary>Where ships may move: the play area inset by the margins reserved for UI.</summary>
        (Vector3 min, Vector3 max) GetPlayfieldRegionBounds(Renderer renderer, ScreenRegionTypes regionType, float buffer = 0f);

        /// <summary>Where an object is still in play: the whole play area, ignoring the UI margins.</summary>
        (Vector3 min, Vector3 max) GetPlayfieldBounds(Renderer renderer, float buffer = 0f);

        /// <summary>The top edge of the play area on the given world plane, from left to right. Takes a
        /// plane instead of a renderer, for placing objects that do not exist yet. The inset is a
        /// fraction of the area's width removed from each side.</summary>
        (Vector3 left, Vector3 right) GetTopEdgeBounds(float planeY, float sideInsetRatio = 0f);

        /// <summary>How much of the renderer has come into the play area from above, from 0 fully hidden
        /// to 1 fully shown, for anything that should react to arriving rather than to being spawned.</summary>
        float GetVisibleFraction(Renderer renderer);

        /// <summary>Takes a point inside the play area, from 0 to 1 on each axis.</summary>
        Vector3 GetPlayfieldWorldPoint(float playfieldX, float playfieldY, float yPosition);
        Vector3 GetScreenPoint(Vector3 worldPosition);
    }

    public class CameraManager : ICameraManager
    {
        [Inject] private readonly IGameRepository _gameRepository;

        /// <summary>Reachable from nowhere else, so the camera keeps a single owner while the shake
        /// maths lives on its own.</summary>
        [Inject] private readonly IScreenShakeService _screenShake;

        [Inject] private readonly IPlayfieldService _playfield;

        private Camera _mainCamera;
        private GameDataConfigSO _gameDataConfig;

        /// <summary>The camera's unshaken position. Bounds are always measured from here.</summary>
        private Vector3 _restPosition;

        public Rect PlayfieldViewportRect => GetPlayfieldViewportRect();

        /// <summary>The camera's current shake offset, subtracted by every bounds query.</summary>
        private Vector3 ShakeOffset => _screenShake.Offset;

        public void Initialize()
        {
            _gameDataConfig = _gameRepository.GetGameDataConfig();

            // Before the rest position is taken: every bounds answer is measured against the size the
            // camera settles on.
            if (!TrySetupCameraToPlayfield())
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
        /// Returns the world-space movement bounds for a renderer within the given play area region,
        /// inset by the renderer's extents, the buffer, and the margins reserved for UI.
        /// </summary>
        /// <remarks>Measured from the camera's resting pose. Callers cache these on first use, so a
        /// query made mid-shake would store the shake offset permanently.</remarks>
        public (Vector3 min, Vector3 max) GetPlayfieldRegionBounds(Renderer renderer, ScreenRegionTypes regionType, float buffer = 0f)
        {
            if (_mainCamera == null || renderer == null || _gameDataConfig == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            Vector3 position = renderer.transform.position;
            Vector3 extents = renderer.bounds.extents;

            float sideMargin = _gameDataConfig.SideMarginRatio;
            Vector3 screenBottomLeft = PlayfieldToWorldPoint(sideMargin, _gameDataConfig.BottomMarginRatio, position.y);
            Vector3 screenTopRight = PlayfieldToWorldPoint(1f - sideMargin, 1f - _gameDataConfig.TopMarginRatio, position.y);
            Vector3 screenDivider = PlayfieldToWorldPoint(0.5f, _gameDataConfig.RegionDividerRatio, position.y);

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
        /// The whole play area, expanded by the renderer's extents so an object is only considered gone
        /// once it has fully left the arena. Deliberately ignores the UI margins, which restrict
        /// where ships may move rather than what is still in play.
        /// </summary>
        public (Vector3 min, Vector3 max) GetPlayfieldBounds(Renderer renderer, float buffer = 0f)
        {
            if (_mainCamera == null || renderer == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            Vector3 position = renderer.transform.position;
            Vector3 extents = renderer.bounds.extents;

            Vector3 bottomLeft = PlayfieldToWorldPoint(0f, 0f, position.y);
            Vector3 topRight = PlayfieldToWorldPoint(1f, 1f, position.y);

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
        /// Uses the whole play area, ignoring the UI margins, since those restrict where ships may move
        /// rather than where the arena starts.
        /// </summary>
        public (Vector3 left, Vector3 right) GetTopEdgeBounds(float planeY, float sideInsetRatio = 0f)
        {
            if (_mainCamera == null)
            {
                return (Vector3.zero, Vector3.zero);
            }

            // Beyond half the width per side the span would invert.
            float inset = Mathf.Clamp(sideInsetRatio, 0f, 0.5f);

            Vector3 leftEdge = PlayfieldToWorldPoint(inset, 1f, planeY);
            Vector3 rightEdge = PlayfieldToWorldPoint(1f - inset, 1f, planeY);

            return (
                new Vector3(leftEdge.x, planeY, leftEdge.z) - ShakeOffset,
                new Vector3(rightEdge.x, planeY, rightEdge.z) - ShakeOffset
            );
        }

        /// <summary>
        /// The play area bounds already include the renderer's extents, so their maximum is the point
        /// where it is fully hidden above the arena, and two extents further in is fully shown. The
        /// fraction is the position between the two.
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

            (Vector3 _, Vector3 maxBounds) = GetPlayfieldBounds(renderer);
            float fullyShownZ = maxBounds.z - extentZ * 2f;

            return Mathf.InverseLerp(maxBounds.z, fullyShownZ, renderer.transform.position.z);
        }

        public Vector3 GetPlayfieldWorldPoint(float playfieldX, float playfieldY, float yPosition)
        {
            if (_mainCamera == null)
            {
                return Vector3.zero;
            }

            return PlayfieldToWorldPoint(playfieldX, playfieldY, yPosition) - ShakeOffset;
        }

        /// <summary>UI is built during scene load, before the initialize phase runs, and a play area not
        /// yet set up would silently answer with the whole screen.</summary>
        private Rect GetPlayfieldViewportRect()
        {
            TrySetupCameraToPlayfield();

            return _playfield.ViewportRect;
        }

        /// <summary>Runs once: the first caller frames the camera, later ones find it already done.</summary>
        private bool TrySetupCameraToPlayfield()
        {
            if (_mainCamera != null)
            {
                return true;
            }

            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                return false;
            }

            _playfield.SetupToPlayfield(_mainCamera);

            return true;
        }

        /// <summary>The one place play area coordinates become world ones. Carries the shake, since
        /// callers subtract it once from the bounds they return.</summary>
        private Vector3 PlayfieldToWorldPoint(float playfieldX, float playfieldY, float yPosition)
        {
            Vector2 viewportPoint = _playfield.ToViewportPoint(playfieldX, playfieldY);

            return _mainCamera.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, yPosition));
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
