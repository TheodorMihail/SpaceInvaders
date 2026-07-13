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

        public string SpaceshipPrefabAddress => _spaceshipPrefabPath;
        public ShipBaseStats BaseStats => _baseStats;
        public ProjectileBehaviourComponent ProjectilePrefab => _projectilePrefab;
        public string ObjectID => SpaceshipID;

        public abstract string SpaceshipID { get; }

        public ShipStats CreateStats() => new ShipStats(_baseStats);
    }
}
