using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "EnemySpaceshipConfig", menuName = "SpaceInvaders/Enemy Spaceship Config")]
    public class EnemySpaceshipConfigSO : SpaceshipConfigSO
    {
        [SerializeField] private int _scoreReward = 10;
        [SerializeField] private EnemyTypes _enemyType;

        public int ScoreReward => _scoreReward;
        public override string SpaceshipID => _enemyType.ToString();
    }
}