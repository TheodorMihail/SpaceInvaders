using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Item pickup that falls down the screen and is collected on player contact.</summary>
    public class ItemPickupBehaviourComponent : ScreenBoundedMovingComponent
    {
        [Inject] private readonly ILootManager _lootManager;

        [SerializeField] private CollisionDetectionComponent _collisionDetection;
        [SerializeField] private float _fallSpeed = 30f;

        private InventoryItemEntry _item;

        public void Initialize(InventoryItemEntry item)
        {
            _item = item;
            _direction = Vector3.back;
            _speed = _fallSpeed;
        }

        public override void OnSpawned()
        {
            base.OnSpawned();
            _collisionDetection.OnTriggerEntered += HandleTriggerEnter;
        }

        public override void OnDespawned()
        {
            base.OnDespawned();
            _collisionDetection.OnTriggerEntered -= HandleTriggerEnter;
        }

        private void HandleTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<IPlayerSpaceship>(out _))
            {
                return;
            }

            _lootManager.CollectItem(_item);
            Despawn();
        }
    }
}
