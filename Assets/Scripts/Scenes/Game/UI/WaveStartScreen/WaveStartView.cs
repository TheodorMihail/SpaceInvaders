using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
            var tween = DOTween.To(() => _waveCanvasGroup.alpha, x => _waveCanvasGroup.alpha = x, 1f, animationDuration);
            await tween.ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            tween = DOTween.To(() => _waveCanvasGroup.alpha, x => _waveCanvasGroup.alpha = x, 0f, animationDuration);
            await tween.ToUniTask(cancellationToken: _cancellationTokenSource.Token);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}
