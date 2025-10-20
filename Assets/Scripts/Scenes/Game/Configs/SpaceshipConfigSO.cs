using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "SpaceshipConfig", menuName = "SpaceInvaders/Spaceship Config")]
    public class SpaceshipConfigSO : ScriptableObject
    {
        [Header("Player Settings")]
        [SerializeField] private int _health = 100;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Combat Settings")]
        [SerializeField] private float _fireRate = 0.5f;

        public float MoveSpeed => _moveSpeed;
        public float FireRate => _fireRate;
        public int Health => _health;
    }
}