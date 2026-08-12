using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SpaceInvaders.Scenes.Preload
{
    [AddressablePath("Screens/SplashScreenView")]
    public class SplashView : View
    {
        [SerializeField] private CanvasGroup _logoCanvasGroup;
        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }

        public async UniTask PlayLogoAnimation(float duration, float startDelay)
        {
            _logoCanvasGroup.alpha = 0;
            await UniTask.Delay((int)startDelay * 1000, cancellationToken: _cancellationTokenSource.Token);
            await _logoCanvasGroup.FadeToAsync(1f, duration / 2, _cancellationTokenSource);
            await _logoCanvasGroup.FadeToAsync(0f, duration / 2, _cancellationTokenSource);
        }
    }
}