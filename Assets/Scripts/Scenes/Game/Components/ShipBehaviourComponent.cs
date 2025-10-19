using BaseArchitecture.Core;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class ShipBehaviourComponent : MonoBehaviour, IPoolableObject
    {
        [Inject] private readonly IInputService _inputService;

        private Vector3 _minBounds;
        private Vector3 _maxBounds;
        private Vector3 _extents;

        public void OnSpawned()
        {
            CalculateBounds();
        }

        public void Initialize()
        {
            _inputService.OnShoot += OnPlayerShoot;
            _inputService.OnMove += OnPlayerMove;
        }

        public void OnDespawned()
        {
            _inputService.OnShoot -= OnPlayerShoot;
            _inputService.OnMove -= OnPlayerMove;
        }

        private void OnPlayerShoot()
        {
            // Implement shooting logic here
        }

        private void OnPlayerMove(Vector3 direction)
        {
            Vector3 newPosition = transform.position + direction;

            newPosition.x = Mathf.Clamp(newPosition.x, _minBounds.x, _maxBounds.x);
            newPosition.y = transform.position.y;
            newPosition.z = Mathf.Clamp(newPosition.z, _minBounds.z, _maxBounds.z);

            transform.position = newPosition;
        }
        

        private void CalculateBounds()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            Renderer renderer = GetComponent<Renderer>();
            _extents = renderer.bounds.extents;
            Vector3 screenBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, transform.position.y));
            Vector3 screenTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, transform.position.y));
            Vector3 screenCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, transform.position.y));

            _minBounds = new Vector3(screenBottomLeft.x + _extents.x, transform.position.y, screenBottomLeft.z + _extents.z);
            _maxBounds = new Vector3(screenTopRight.x - _extents.x, transform.position.y, screenCenter.z - _extents.z);
        }
    }
}
