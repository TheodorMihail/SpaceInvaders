using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "EnemySpaceshipConfig", menuName = "SpaceInvaders/Enemy Spaceship Config")]
    public class EnemySpaceshipConfigSO : SpaceshipConfigSO
    {
        [SerializeField] private int _scoreReward = 10;
        [SerializeField] private EnemyTypes _enemyType;
        [SerializeField] private EnemyCategory _category;

        public int ScoreReward => _scoreReward;
        public EnemyTypes EnemyType => _enemyType;
        public EnemyCategory Category => _category;
        public override string SpaceshipID => _enemyType.ToString();
    }
}