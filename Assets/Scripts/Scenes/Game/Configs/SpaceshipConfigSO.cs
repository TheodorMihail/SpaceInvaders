using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public abstract class SpaceshipConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Spaceship Settings")]
        [SerializeField] private string _spaceshipPrefabPath;
        [SerializeField] private int _health = 100;
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Combat Settings")]
        [SerializeField] private float _fireRate = 0.5f;
        [SerializeField] private ProjectileBehaviourComponent _projectilePrefab;
        [SerializeField] private int _projectileDamage = 10;
        [SerializeField] private float _projectileSpeed = 15f;


        public string SpaceshipPrefabAddress => _spaceshipPrefabPath;
        public int Health => _health;
        public float MoveSpeed => _moveSpeed;
        public float FireRate => _fireRate;
        public ProjectileBehaviourComponent ProjectilePrefab => _projectilePrefab;
        public int ProjectileDamage => _projectileDamage;
        public float ProjectileSpeed => _projectileSpeed;
        public string ObjectID => SpaceshipID;

        public abstract string SpaceshipID { get; }
    }
}