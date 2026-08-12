using System;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [RequireComponent(typeof(Collider))]
    public class CollisionDetectionComponent : MonoBehaviour
    {
        public event Action<Collider> OnTriggerEntered;

        private void OnDestroy()
        {
            OnTriggerEntered = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEntered?.Invoke(other);
        }
    }
}
