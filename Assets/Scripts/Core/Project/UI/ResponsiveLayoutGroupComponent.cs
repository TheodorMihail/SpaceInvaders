using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Sizes the children of a vertical layout group, and the gaps between them, from the panel's
    /// current size. LayoutElement only supports a fixed or fully flexible preferred size, so an
    /// element that must scale in proportion needs its size driven. Children share a width and keep
    /// their own aspect ratios.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ResponsiveLayoutGroupComponent : MonoBehaviour
    {
        /// <summary>One child's authored aspect ratio, stored separately from its driven size.</summary>
        [Serializable]
        private struct ChildAspect
        {
            public RectTransform Child;
            public float Aspect;
        }

        [SerializeField] private HorizontalOrVerticalLayoutGroup _layoutGroup;

        [Tooltip("Child width as a fraction of this panel's width.")]
        [SerializeField, Range(0.05f, 1f)] private float _childWidthRatio = 0.8f;

        [Tooltip("Gap between children as a fraction of the average child height, so the gaps grow and " +
                 "shrink with the children rather than staying put.")]
        [SerializeField, Range(0f, 1f)] private float _spacingRatio = 0.2f;

        [Tooltip("Used for a child with no size to read a shape from, as width over height.")]
        [SerializeField, Min(0.05f)] private float _fallbackAspect = 3f;

        [Tooltip("Upper bound so a large panel does not blow the children up. 0 removes the cap.")]
        [SerializeField, Min(0f)] private float _maxChildWidth;

        [Tooltip("Shrinks children so every one of them fits the panel height, spacing included.")]
        [SerializeField] private bool _fitToHeight = true;

        [Tooltip("Each child's shape, captured once. Driving a child's size would otherwise overwrite " +
                 "the very ratio it is meant to preserve. Author the sizes, then Recapture Child Aspects.")]
        [SerializeField] private List<ChildAspect> _childAspects = new();

        /// <summary>Reused rather than allocated, since this runs on every layout pass.</summary>
        private readonly List<LayoutElement> _children = new();

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;

            if (_layoutGroup == null)
            {
                _layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            }
        }

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>Unity's own callback for "my rect changed", which covers window resizes and
        /// orientation changes without polling for either.</summary>
        private void OnRectTransformDimensionsChange()
        {
            Refresh();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_layoutGroup == null)
            {
                _layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            }

            Refresh();
        }
#endif

        /// <summary>Re-reads every child's aspect ratio from its current size. Run after authoring the
        /// children, since their size is driven from the captured value afterwards.</summary>
        [ContextMenu("Recapture Child Aspects")]
        public void RecaptureChildAspects()
        {
            _childAspects.Clear();
            Refresh();
        }

        private void Refresh()
        {
            if (_layoutGroup == null || _rectTransform == null)
            {
                return;
            }

            CollectChildren();

            if (_children.Count == 0)
            {
                return;
            }

            Rect rect = _rectTransform.rect;
            float availableWidth = rect.width - _layoutGroup.padding.left - _layoutGroup.padding.right;

            if (availableWidth <= 0f)
            {
                return;
            }

            float childWidth = availableWidth * _childWidthRatio;

            if (_maxChildWidth > 0f)
            {
                childWidth = Mathf.Min(childWidth, _maxChildWidth);
            }

            // Every child's height is this shared width over its own aspect, so the sum of the inverse
            // aspects is what one unit of width costs in height across the whole stack.
            float inverseAspectSum = GetInverseAspectSum();

            if (_fitToHeight)
            {
                childWidth = Mathf.Min(childWidth, GetWidthBudget(rect, inverseAspectSum));
            }

            ApplySpacing(childWidth, inverseAspectSum);
            ApplyChildSizes(childWidth);
        }

        /// <summary>
        /// The widest the children can be for all of them plus the gaps to fit. Solved rather than
        /// measured, because the gaps are a fraction of the heights being calculated.
        /// </summary>
        private float GetWidthBudget(Rect rect, float inverseAspectSum)
        {
            float available = rect.height - _layoutGroup.padding.top - _layoutGroup.padding.bottom;

            if (available <= 0f || inverseAspectSum <= 0f)
            {
                return float.MaxValue;
            }

            // Total height is width times the inverse aspect sum, and the gaps add another ratio of the
            // average height for each of the count minus one of them.
            float spacingFactor = 1f + _spacingRatio * (_children.Count - 1) / _children.Count;

            return available / (inverseAspectSum * spacingFactor);
        }

        private float GetInverseAspectSum()
        {
            float sum = 0f;

            foreach (LayoutElement element in _children)
            {
                sum += 1f / GetAspect(element);
            }

            return sum;
        }

        /// <summary>
        /// The captured aspect ratio for a child, capturing it on first use. Never read back from the
        /// driven size, or one bad pass would permanently store a wrong ratio.
        /// </summary>
        private float GetAspect(LayoutElement element)
        {
            var child = (RectTransform)element.transform;

            foreach (ChildAspect captured in _childAspects)
            {
                if (captured.Child == child)
                {
                    return captured.Aspect > 0f ? captured.Aspect : _fallbackAspect;
                }
            }

            float aspect = CaptureAspect(element, child);
            _childAspects.Add(new ChildAspect { Child = child, Aspect = aspect });

            return aspect;
        }

        private float CaptureAspect(LayoutElement element, RectTransform child)
        {
            if (element.preferredWidth > 0f && element.preferredHeight > 0f)
            {
                return element.preferredWidth / element.preferredHeight;
            }

            Rect childRect = child.rect;

            if (childRect.width > 0f && childRect.height > 0f)
            {
                return childRect.width / childRect.height;
            }

            return _fallbackAspect;
        }

        private void CollectChildren()
        {
            _children.Clear();

            foreach (RectTransform child in _rectTransform)
            {
                if (child.gameObject.activeSelf && child.TryGetComponent(out LayoutElement element))
                {
                    _children.Add(element);
                }
            }
        }

        /// <summary>Sized against the average child height, so one very tall child does not open the
        /// gaps around every other one.</summary>
        private void ApplySpacing(float childWidth, float inverseAspectSum)
        {
            float averageHeight = childWidth * inverseAspectSum / _children.Count;
            float spacing = averageHeight * _spacingRatio;

            if (Mathf.Abs(_layoutGroup.spacing - spacing) < 0.01f)
            {
                return;
            }

            _layoutGroup.spacing = spacing;
        }

        private void ApplyChildSizes(float childWidth)
        {
            foreach (LayoutElement element in _children)
            {
                float childHeight = childWidth / GetAspect(element);

                // Writing a preferred size marks the layout dirty, which comes straight back here.
                // Bailing when nothing changed is what stops that becoming an endless rebuild.
                if (Mathf.Abs(element.preferredWidth - childWidth) < 0.01f
                    && Mathf.Abs(element.preferredHeight - childHeight) < 0.01f)
                {
                    continue;
                }

                element.preferredWidth = childWidth;
                element.preferredHeight = childHeight;
            }
        }
    }
}
