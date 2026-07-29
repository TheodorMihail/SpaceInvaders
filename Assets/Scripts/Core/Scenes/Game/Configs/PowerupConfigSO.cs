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

        public virtual float Duration => _duration;
        public virtual int DropWeight => _dropWeight;
        public virtual PowerupBehaviourComponent PickupPrefab => _pickupPrefab;
        public virtual Sprite Icon => _icon;
        public virtual string ObjectID => PowerupType.ToString();

        public abstract PowerupTypes PowerupType { get; }
    }
}
