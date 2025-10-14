using System.Threading;
using Base.Systems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace SpaceInvaders.Scenes.GamePreload
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

        public async UniTask PlayLogoAnimation(float duration)
        {
            float loadingPercentage = 0;
            _logoCanvasGroup.alpha = 0;

            await UniTask.Delay(1000, cancellationToken: _cancellationTokenSource.Token);
            
            var tween = DOTween.To(() => loadingPercentage, x => loadingPercentage = x, 100, duration)
                .OnUpdate(() =>
                {
                    _logoCanvasGroup.alpha = loadingPercentage / 100;
                });

            await tween.ToUniTask(cancellationToken: _cancellationTokenSource.Token);
            _logoCanvasGroup.alpha = 1;

            //Adding another second of delay to see the logo fully visible
            await UniTask.Delay(1000, cancellationToken: _cancellationTokenSource.Token);
        }
        
        private void OnDestroy()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}