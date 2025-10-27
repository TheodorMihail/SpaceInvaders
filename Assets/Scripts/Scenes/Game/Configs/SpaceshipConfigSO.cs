using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "SpaceshipConfig", menuName = "SpaceInvaders/Spaceship Config")]
    public class SpaceshipConfigSO : ScriptableObject
    {
        [Header("Health Settings")]
        [SerializeField] private int _health = 100;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Combat Settings")]
        [SerializeField] private float _fireRate = 0.5f;
        [SerializeField] private ProjectileBehaviourComponent _projectilePrefab;
        [SerializeField] private int _projectileDamage = 10;
        [SerializeField] private float _projectileSpeed = 15f;

        public int Health => _health;
        public float MoveSpeed => _moveSpeed;
        public float FireRate => _fireRate;
        public ProjectileBehaviourComponent ProjectilePrefab => _projectilePrefab;
        public int ProjectileDamage => _projectileDamage;
        public float ProjectileSpeed => _projectileSpeed;
    }
}