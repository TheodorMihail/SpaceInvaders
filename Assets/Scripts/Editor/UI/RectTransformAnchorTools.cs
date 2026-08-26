using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpaceInvaders.Editor
{
    /// <summary>
    /// Converts between absolute offsets and relative anchors on the selected RectTransforms. Unity
    /// only offers fixed anchor presets, so a hand-sized element cannot otherwise scale with its
    /// parent.
    /// </summary>
    internal static class RectTransformAnchorTools
    {
        private const string AnchorsToCornersItem = "SpaceInvaders/UI/Anchors to Corners %#k";
        private const string CornersToAnchorsItem = "SpaceInvaders/UI/Corners to Anchors %#l";

        /// <summary>
        /// Moves the anchors onto the rect's current corners and zeroes the offsets, so the element
        /// keeps its size and then scales with the parent. Scale is applied because offsets describe
        /// the unscaled rect. Neither the scale nor the rect size is changed.
        /// </summary>
        [MenuItem(AnchorsToCornersItem)]
        private static void AnchorsToCorners()
        {
            foreach (RectTransform rectTransform in GetSelectedRectTransforms())
            {
                if (rectTransform.parent is not RectTransform parent)
                {
                    Debug.LogWarning($"[AnchorTools] '{rectTransform.name}' has no RectTransform parent to anchor against. Skipping.");
                    continue;
                }

                Vector2 parentSize = parent.rect.size;

                // A parent with no size has not been laid out yet, and dividing by it would throw the
                // anchors to infinity.
                if (Mathf.Approximately(parentSize.x, 0f) || Mathf.Approximately(parentSize.y, 0f))
                {
                    Debug.LogWarning($"[AnchorTools] Parent of '{rectTransform.name}' has no size yet. Skipping.");
                    continue;
                }

                Vector3 scale = rectTransform.localScale;

                if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f))
                {
                    Debug.LogWarning($"[AnchorTools] '{rectTransform.name}' is scaled to nothing on an axis. Skipping.");
                    continue;
                }

                Undo.RecordObject(rectTransform, "Anchors to Corners");

                Vector2 size = rectTransform.rect.size;
                Vector2 pivot = rectTransform.pivot;

                // Scaling happens about the pivot, so the corners the eye sees sit this far from the
                // ones the offsets describe. At a scale of 1 both terms fall to zero.
                var scaledMinInset = new Vector2(
                    pivot.x * size.x * (1f - scale.x),
                    pivot.y * size.y * (1f - scale.y));

                var scaledMaxInset = new Vector2(
                    (1f - pivot.x) * size.x * (1f - scale.x),
                    (1f - pivot.y) * size.y * (1f - scale.y));

                // Offsets are measured from the anchors already in place, so they are folded into them
                // rather than replacing them. That keeps the result correct for a partially anchored rect.
                rectTransform.anchorMin = new Vector2(
                    rectTransform.anchorMin.x + (rectTransform.offsetMin.x + scaledMinInset.x) / parentSize.x,
                    rectTransform.anchorMin.y + (rectTransform.offsetMin.y + scaledMinInset.y) / parentSize.y);

                rectTransform.anchorMax = new Vector2(
                    rectTransform.anchorMax.x + (rectTransform.offsetMax.x - scaledMaxInset.x) / parentSize.x,
                    rectTransform.anchorMax.y + (rectTransform.offsetMax.y - scaledMaxInset.y) / parentSize.y);

                // Not zeroed: the offsets take back exactly what the anchors just gained, so the rect
                // keeps the size and position it has now. Zeroing them would shrink the rect onto the
                // visible corners, and the scale would then shrink it again from there.
                // At a scale of 1 both insets are zero, so this is the plain corner snap.
                rectTransform.offsetMin = -scaledMinInset;
                rectTransform.offsetMax = scaledMaxInset;

                EditorUtility.SetDirty(rectTransform);
            }
        }

        /// <summary>The inverse: collapses the anchors to a single point and restates the rect as
        /// absolute offsets, for an element that should stop scaling with its parent.</summary>
        [MenuItem(CornersToAnchorsItem)]
        private static void CornersToAnchors()
        {
            foreach (RectTransform rectTransform in GetSelectedRectTransforms())
            {
                Undo.RecordObject(rectTransform, "Corners to Anchors");

                Vector2 anchorCentre = (rectTransform.anchorMin + rectTransform.anchorMax) * 0.5f;
                Vector2 size = rectTransform.rect.size;
                Vector2 position = rectTransform.anchoredPosition;

                rectTransform.anchorMin = anchorCentre;
                rectTransform.anchorMax = anchorCentre;
                rectTransform.sizeDelta = size;
                rectTransform.anchoredPosition = position;

                EditorUtility.SetDirty(rectTransform);
            }
        }

        [MenuItem(AnchorsToCornersItem, true)]
        [MenuItem(CornersToAnchorsItem, true)]
        private static bool ValidateSelection()
        {
            return GetSelectedRectTransforms().Count > 0;
        }

        private static List<RectTransform> GetSelectedRectTransforms()
        {
            var rectTransforms = new List<RectTransform>();

            foreach (Transform selected in Selection.transforms)
            {
                if (selected is RectTransform rectTransform)
                {
                    rectTransforms.Add(rectTransform);
                }
            }

            return rectTransforms;
        }
    }
}
