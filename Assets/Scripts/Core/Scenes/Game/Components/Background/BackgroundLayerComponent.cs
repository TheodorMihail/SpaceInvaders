using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// One depth layer of the scrolling background. Drifts its pieces down the screen and lifts
    /// whichever piece has left the bottom back onto the top of the stack, so the layer never runs out.
    /// The multiplier is what separates one layer from another: the further away a layer reads, the
    /// slower it should travel.
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

        /// <summary>Pieces are read off the hierarchy, so a layer can be re-authored without code changes.</summary>
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

        /// <summary>Travels the layer down by the given distance, recycling any piece that runs off the bottom.</summary>
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

        /// <summary>The authored stack defines the loop: pieces are spaced by their own height, so
        /// travelling one spacing past the lowest slot lands a piece exactly on the highest one.</summary>
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
