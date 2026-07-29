using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public abstract class SpaceshipConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Spaceship Settings")]
        [SerializeField] private string _spaceshipPrefabPath;
        [SerializeField] private ShipBaseStats _baseStats;

        [Header("Combat Settings")]
        [SerializeField] private ProjectileBehaviourComponent _projectilePrefab;

        [Header("VFX Settings")]
        [SerializeField] private VFXBehaviourComponent _destroyVFXPrefab;
        [SerializeField] private VFXBehaviourComponent _hitVFXPrefab;

        public string SpaceshipPrefabAddress => _spaceshipPrefabPath;
        public ShipBaseStats BaseStats => _baseStats;
        public ProjectileBehaviourComponent ProjectilePrefab => _projectilePrefab;
        public VFXBehaviourComponent DestroyVFXPrefab => _destroyVFXPrefab;
        public VFXBehaviourComponent HitVFXPrefab => _hitVFXPrefab;
        public string ObjectID => SpaceshipID;

        public abstract string SpaceshipID { get; }

        public ShipStats CreateStats()
        {
            return new ShipStats(_baseStats);
        }
    }
}
