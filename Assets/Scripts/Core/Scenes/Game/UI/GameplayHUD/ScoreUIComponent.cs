using System.Threading;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Score readout that counts up to each new total rather than snapping to it.</summary>
    public class ScoreUIComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private string _scoreString = "Score: {0}";
        [SerializeField] private float _countUpDuration = 0.5f;

        private CancellationTokenSource _countUpCancellationTokenSource;
        private int _currentScore;

        public void Initialize(int score)
        {
            CancelCountUp();

            _currentScore = score;
            FormatScore(score);
        }

        private void OnDestroy()
        {
            CancelCountUp();
        }

        public async void UpdateScore(int score)
        {
            CancelCountUp();
            _countUpCancellationTokenSource = new CancellationTokenSource();

            await _scoreText.CountdownAsync(_currentScore, score, _countUpDuration, FormatScore, _countUpCancellationTokenSource);
            _currentScore = score;
        }

        private void FormatScore(int score)
        {
            _scoreText.text = string.Format(_scoreString, score);
        }

        private void CancelCountUp()
        {
            _countUpCancellationTokenSource?.CancelAndDispose();
            _countUpCancellationTokenSource = null;
        }
    }
}
