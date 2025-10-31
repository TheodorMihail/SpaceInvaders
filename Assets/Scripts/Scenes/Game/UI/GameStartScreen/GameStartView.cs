using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/GameStartScreenView")]
    public class GameStartView : View
    {
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private TextMeshProUGUI _pressAnyKeyText;
        [SerializeField] private string _startString = "START!";

        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _countdownText.gameObject.SetActive(false);
            _pressAnyKeyText.gameObject.SetActive(true);
        }

        public async UniTask StartCountdownAnimation(int countdownValue, float endDelay)
        {
            _countdownText.gameObject.SetActive(true);
            _pressAnyKeyText.gameObject.SetActive(false);
            _countdownText.text = countdownValue.ToString();

            await _countdownText.CountdownAsync(countdownValue, 1, countdownValue, null, _cancellationTokenSource);

            _countdownText.text = _startString;
            await UniTask.Delay((int)endDelay * 1000, cancellationToken: _cancellationTokenSource.Token);
        }
        
        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}
