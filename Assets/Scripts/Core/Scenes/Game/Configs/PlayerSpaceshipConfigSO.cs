using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "PlayerSpaceshipConfig", menuName = "SpaceInvaders/Player Spaceship Config")]
    public class PlayerSpaceshipConfigSO : SpaceshipConfigSO
    {
        [SerializeField] private PlayerTypes _playerType;
        public override string SpaceshipID => _playerType.ToString();
    }
}