using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public enum SoundCategoryTypes
    {
        Music = 0,
        SFX = 1,
        UI = 2
    }

    public enum SoundTypes
    {
        //Music
        MenuMusic = 0,
        GameplayMusic = 1,

        //UI
        ButtonClick = 10,

        //Gameplay
        ShipShoot = 20,
        ShipDamaged = 21,
        EnemyDestroyed = 22,
        PlayerDestroyed = 23,
        PowerupPickup = 24,
        PowerupExpired = 25,
        ItemCollected = 26,
        ItemDropped = 27,
        PowerupDropped = 28,

        //Level
        BossSpawned = 40,
        WaveStarted = 41,
        LevelCompleted = 42,
        GameOver = 43
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
