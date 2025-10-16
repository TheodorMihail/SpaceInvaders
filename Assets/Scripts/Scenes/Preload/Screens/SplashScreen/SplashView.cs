using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace SpaceInvaders.Scenes.Preload
{
    [AddressablePath("Screens/SplashView")]
    public class SplashView : View
    {
        [SerializeField] private CanvasGroup _logoCanvasGroup;
        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async UniTask PlayLogoAnimation(float duration, float startDelay, float endDelay)
        {
            _logoCanvasGroup.alpha = 0;
            await UniTask.Delay((int)startDelay * 1000, cancellationToken: _cancellationTokenSource.Token);
            var tween = DOTween.To(() => _logoCanvasGroup.alpha, x => _logoCanvasGroup.alpha = x, 1f, duration);
            await tween.ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            _logoCanvasGroup.alpha = 1;

            //Adding another delay to see the logo fully visible
            await UniTask.Delay((int)endDelay * 1000, cancellationToken: _cancellationTokenSource.Token);
        }
        
        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}