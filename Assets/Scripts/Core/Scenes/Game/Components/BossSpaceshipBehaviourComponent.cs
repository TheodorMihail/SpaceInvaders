using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    public class BossSpaceshipBehaviourComponent : EnemySpaceshipBehaviourComponent
    {
        [Inject] private readonly IPlayerManager _playerManager;

        [SerializeField] private float _spreadAngleDegrees = 20f;

        private int _shotCounter;

        public override void OnDespawned()
        {
            base.OnDespawned();
            _shotCounter = 0;
        }

        protected override IEnumerable<Vector3> GetShotDirections()
        {
            _shotCounter++;

            if (_shotCounter % 2 == 0)
            {
                yield return GetAimedDirection();
            }
            else
            {
                yield return RotateAroundY(Vector3.back, -_spreadAngleDegrees);
                yield return Vector3.back;
                yield return RotateAroundY(Vector3.back, _spreadAngleDegrees);
            }
        }

        private Vector3 GetAimedDirection()
        {
            Vector3 toPlayer = _playerManager.PlayerPosition - transform.position;
            toPlayer.y = 0f;

            return toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector3.back;
        }

        private static Vector3 RotateAroundY(Vector3 direction, float angleDegrees)
        {
            float angleInRadians = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleInRadians);
            float sin = Mathf.Sin(angleInRadians);

            return new Vector3(
                direction.x * cos - direction.z * sin,
                direction.y,
                direction.x * sin + direction.z * cos);
        }
    }
}
