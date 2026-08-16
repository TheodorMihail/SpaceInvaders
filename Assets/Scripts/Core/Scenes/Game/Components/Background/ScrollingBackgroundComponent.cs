using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>
    /// Drives every background layer from one speed, so the whole flight can be sped up or slowed
    /// down without retuning the layers against each other. Layers are discovered from the hierarchy,
    /// so one dropped onto this object can never miss the scroll.
    /// </summary>
    public class ScrollingBackgroundComponent : MonoBehaviour
    {
        [Tooltip("Local units the background travels down per second, before each layer's multiplier.")]
        [SerializeField] private float _scrollSpeed = 1f;

        private BackgroundLayerComponent[] _layers;

        private void Awake()
        {
            _layers = GetComponentsInChildren<BackgroundLayerComponent>();

            if (_layers.Length == 0)
            {
                this.LogError("No background layers found. Nothing will scroll.");
            }
        }

        // Time scaled, so the flight halts with the rest of the game while paused.
        private void Update()
        {
            float distance = _scrollSpeed * Time.deltaTime;

            foreach (BackgroundLayerComponent layer in _layers)
            {
                layer.Scroll(distance);
            }
        }
    }
}
