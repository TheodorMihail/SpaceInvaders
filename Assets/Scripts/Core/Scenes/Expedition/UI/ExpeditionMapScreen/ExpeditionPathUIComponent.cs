using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    /// <summary>A link between two nodes, drawn as one stretched and rotated rect.</summary>
    public class ExpeditionPathUIComponent : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _thickness = 6f;

        public void Initialize(Vector2 fromAnchoredPosition, Vector2 toAnchoredPosition)
        {
            Vector2 delta = toAnchoredPosition - fromAnchoredPosition;

            _rectTransform.anchoredPosition = fromAnchoredPosition + delta * 0.5f;
            _rectTransform.sizeDelta = new Vector2(delta.magnitude, _thickness);
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }
    }
}
