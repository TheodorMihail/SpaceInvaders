using System.Threading;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("HUD/GameplayHUDView")]
    public class GameplayHUDView : View
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _levelText;

        [SerializeField] private string _scoreString = "Score: {0}";
        [SerializeField] private string _levelString = "Level: {0}";

        private CancellationTokenSource _scoreCancellationTokenSource;
        private int _currentScore = 0;

        public void Setup(int levelNumber)
        {
            _levelText.text = string.Format(_levelString, levelNumber);
            FormatScore(0);
        }

        public async void UpdateScore(int score)
        {
            _scoreCancellationTokenSource?.CancelAndDispose();
            _scoreCancellationTokenSource = new CancellationTokenSource();

            await _scoreText.CountdownAsync(_currentScore, score, 0.5f, FormatScore, _scoreCancellationTokenSource);
            _currentScore = score;
        }

        private void FormatScore(int score)
        {
            _scoreText.text = string.Format(_scoreString, score);
        }

        private void OnDestroy()
        {
            _scoreCancellationTokenSource?.CancelAndDispose();
        }
    }
}
