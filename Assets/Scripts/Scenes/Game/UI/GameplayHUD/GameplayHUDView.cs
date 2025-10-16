using BaseArchitecture.Core;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("HUD/GameplayHUDView")]
    public class GameplayHUDView : View
    {
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        
        [SerializeField] private string _scoreString = "Score: {0}";
        [SerializeField] private string _livesString = "Lives: {0}";

        public void UpdateLives(int lives)
        {
            _livesText.text = string.Format(_livesString, lives);
        }

        public void UpdateScore(int score)
        {
            _scoreText.text = string.Format(_scoreString, score);
        }
    }
}
