using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    public enum PowerupTypes
    {
        Invincibility,
        Heal,
        DamageBoost,
        RapidFire,
        SpreadShot
    }

    public abstract class PowerupConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Powerup Settings")]
        [SerializeField] private float _duration;
        [SerializeField] private int _dropWeight = 1;
        [SerializeField] private PowerupBehaviourComponent _pickupPrefab;
        [SerializeField] private Sprite _icon;

        public float Duration => _duration;
        public int DropWeight => _dropWeight;
        public PowerupBehaviourComponent PickupPrefab => _pickupPrefab;
        public Sprite Icon => _icon;
        public string ObjectID => PowerupType.ToString();

        public abstract PowerupTypes PowerupType { get; }
    }
}
