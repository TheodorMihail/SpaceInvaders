using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/WaveStartScreenView")]
    public class WaveStartView : View
    {
        [SerializeField] private CanvasGroup _waveCanvasGroup;
        [SerializeField] private TextMeshProUGUI _waveNumberText;
        [SerializeField] private string _waveString = $"Wave {0}";

        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _waveNumberText.alpha = 1f;
        }

        public void SetWaveNumber(int waveNumber)
        {
            _waveNumberText.text =  string.Format(_waveString, Mathf.RoundToInt(waveNumber));
        }

        public async UniTask PlayAnimation(float animationDuration)
        {
            _waveCanvasGroup.alpha = 0f;
            await _waveCanvasGroup.FadeToAsync(1f, animationDuration, _cancellationTokenSource);
            await _waveCanvasGroup.FadeToAsync(0f, animationDuration, _cancellationTokenSource);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}
