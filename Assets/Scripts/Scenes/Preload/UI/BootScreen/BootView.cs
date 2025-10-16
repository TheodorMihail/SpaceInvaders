using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Preload
{
    [AddressablePath("Screens/BootView")]
    public class BootView : View
    {
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private string _loadingString = "{0}%";
        [SerializeField] private string _loadingFinishedString = "Complete!";

        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async UniTask PlayLoadingAnimation(float duration, float endDelay)
        {
            float loadingPercentage = 0;
            var tween = DOTween.To(() => loadingPercentage, x => loadingPercentage = x, 100, duration)
                .OnUpdate(() =>
                {
                    _loadingText.text = string.Format(_loadingString, Mathf.RoundToInt(loadingPercentage));
                });

            await tween.ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            _loadingText.text = _loadingFinishedString;

            //Adding another delay to see the loading finished text
            await UniTask.Delay((int)endDelay * 1000, cancellationToken: _cancellationTokenSource.Token);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}