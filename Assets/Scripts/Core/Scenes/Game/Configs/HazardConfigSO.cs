using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Values are pinned: configs serialize this enum by index.</summary>
    public enum HazardTypes
    {
        Asteroid = 0
    }

    /// <summary>
    /// What a hazard is. How often it turns up is authored per wave instead, so the same hazard can
    /// be a rare nuisance in one level and constant pressure in another.
    /// </summary>
    [CreateAssetMenu(fileName = "HazardConfig", menuName = "SpaceInvaders/Hazards/Hazard Config")]
    public class HazardConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Hazard Settings")]
        [SerializeField] private HazardTypes _hazardType;
        [SerializeField] private BaseHazardBehaviourComponent _hazardPrefab;
        [SerializeField] private HazardBaseStats _baseStats;

        [Header("VFX Settings")]
        [SerializeField] private VFXBehaviourComponent _destroyVFXPrefab;
        [SerializeField] private VFXBehaviourComponent _hitVFXPrefab;

        public virtual HazardTypes HazardType => _hazardType;
        public virtual BaseHazardBehaviourComponent HazardPrefab => _hazardPrefab;
        public virtual HazardBaseStats BaseStats => _baseStats;
        public virtual VFXBehaviourComponent DestroyVFXPrefab => _destroyVFXPrefab;
        public virtual VFXBehaviourComponent HitVFXPrefab => _hitVFXPrefab;

        public virtual string ObjectID => _hazardType.ToString();

        public HazardStats CreateStats()
        {
            return new HazardStats(_baseStats);
        }
    }
}
