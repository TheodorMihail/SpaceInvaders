using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Sizes a GridLayoutGroup's cells from the width it actually has, for a fixed number of columns.
    /// The layout group writes cellSize onto every child each pass, so anchors on the cells themselves
    /// can never make a grid responsive: the cell size is the only thing worth driving.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
    public class ResponsiveGridComponent : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup _grid;

        [Tooltip("Columns to fit across the width, whatever that width turns out to be.")]
        [SerializeField, Min(1)] private int _columns = 4;

        [Tooltip("Keeps cells square. Off uses the aspect below, as width divided by height.")]
        [SerializeField] private bool _squareCells = true;

        [SerializeField, Min(0.05f)] private float _cellAspect = 1f;

        [Tooltip("Upper bound so a wide container does not blow the cells up. 0 removes the cap.")]
        [SerializeField, Min(0f)] private float _maxCellWidth;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;

            if (_grid == null)
            {
                _grid = GetComponent<GridLayoutGroup>();
            }
        }

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>Unity's own callback for "my rect changed", which covers the window resizing, the
        /// scroll view being laid out, and orientation changes, without polling for any of them.</summary>
        private void OnRectTransformDimensionsChange()
        {
            Refresh();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_grid == null)
            {
                _grid = GetComponent<GridLayoutGroup>();
            }

            Refresh();
        }
#endif

        private void Refresh()
        {
            if (_grid == null || _rectTransform == null)
            {
                return;
            }

            float available = _rectTransform.rect.width
                - _grid.padding.left
                - _grid.padding.right
                - _grid.spacing.x * (_columns - 1);

            if (available <= 0f)
            {
                return;
            }

            float cellWidth = available / _columns;

            if (_maxCellWidth > 0f)
            {
                cellWidth = Mathf.Min(cellWidth, _maxCellWidth);
            }

            float cellHeight = _squareCells ? cellWidth : cellWidth / _cellAspect;
            var cellSize = new Vector2(cellWidth, cellHeight);

            // Writing cellSize marks the layout dirty, which comes straight back here. Bailing when
            // nothing actually changed is what stops that becoming an endless rebuild.
            if ((_grid.cellSize - cellSize).sqrMagnitude < 0.01f && _grid.constraintCount == _columns)
            {
                return;
            }

            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = _columns;
            _grid.cellSize = cellSize;
        }
    }
}
