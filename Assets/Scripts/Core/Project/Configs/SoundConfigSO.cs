using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public enum SoundCategoryTypes
    {
        Music,
        SFX,
        UI
    }

    public enum SoundTypes
    {
        MenuMusic,
        GameplayMusic,
        ButtonClick,
        ShipShoot,
        ShipDamaged,
        EnemyDestroyed,
        PlayerDestroyed,
        PowerupPickup,
        PowerupExpired,
        BossSpawned,
        WaveStarted,
        LevelCompleted,
        GameOver
    }

    [CreateAssetMenu(fileName = "SoundConfig", menuName = "SpaceInvaders/Sounds/Sound Config")]
    public class SoundConfigSO : ScriptableObject, IRepositoryObject
    {
        [SerializeField] private SoundTypes _type;
        [SerializeField] private SoundCategoryTypes _category;
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private bool _loop;

        public SoundTypes Type => _type;
        public SoundCategoryTypes Category => _category;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
        public bool Loop => _loop;
        public string ObjectID => _type.ToString();
    }
}
