using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// One depth layer of the scrolling background. Moves its pieces down the screen and recycles any
    /// that leave the bottom back to the top. The speed multiplier sets the layer's apparent depth.
    /// </summary>
    public class BackgroundLayerComponent : MonoBehaviour
    {
        private const int MinimumPieceCount = 2;

        [Tooltip("Scales this layer's travel against the background's scroll speed. Lower reads as further away.")]
        [SerializeField] private float _speedMultiplier = 1f;

        private readonly List<Transform> _pieces = new List<Transform>();

        private float _loopLocalLength;
        private float _wrapLocalY;
        private bool _isScrollable;

        /// <summary>Pieces are read from the hierarchy, so a layer can be re-authored without code.</summary>
        private void Awake()
        {
            _pieces.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                _pieces.Add(transform.GetChild(i));
            }

            if (_pieces.Count < MinimumPieceCount)
            {
                this.LogError($"Needs at least {MinimumPieceCount} pieces to scroll endlessly, found {_pieces.Count}.");
                return;
            }

            MeasureStack();
        }

        public void Scroll(float distance)
        {
            if (!_isScrollable)
            {
                return;
            }

            float step = distance * _speedMultiplier;

            foreach (Transform piece in _pieces)
            {
                Vector3 localPosition = piece.localPosition;
                localPosition.y -= step;

                // Looped rather than branched so a long frame cannot leave a piece stranded below the stack.
                while (localPosition.y <= _wrapLocalY)
                {
                    localPosition.y += _loopLocalLength;
                }

                piece.localPosition = localPosition;
            }
        }

        /// <summary>Pieces are spaced by their own height, so moving one spacing past the lowest slot
        /// lands a piece exactly on the highest.</summary>
        private void MeasureStack()
        {
            float lowestLocalY = float.MaxValue;
            float highestLocalY = float.MinValue;

            foreach (Transform piece in _pieces)
            {
                float pieceLocalY = piece.localPosition.y;
                lowestLocalY = Mathf.Min(lowestLocalY, pieceLocalY);
                highestLocalY = Mathf.Max(highestLocalY, pieceLocalY);
            }

            float spacing = (highestLocalY - lowestLocalY) / (_pieces.Count - 1);

            if (spacing <= 0f)
            {
                this.LogError("Pieces sit on top of each other. Stack them one piece height apart along local Y.");
                return;
            }

            _loopLocalLength = spacing * _pieces.Count;
            _wrapLocalY = lowestLocalY - spacing;
            _isScrollable = true;
        }
    }
}
