using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/GameAnnouncerHUDView")]
    public class GameAnnouncerHUDView : View
    {
        [SerializeField] private CanvasGroup _textCanvasGroup;
        [SerializeField] private TextMeshProUGUI _displayText;

        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _displayText.alpha = 1f;
            _textCanvasGroup.alpha = 0f;
        }

        public void SetDisplayText(string text)
        {
            _displayText.text = text;
        }

        public async UniTask PlayAnimation(float animationDuration)
        {
            _textCanvasGroup.alpha = 0f;
            
            try
            {
                await _textCanvasGroup.FadeToAsync(1f, animationDuration, _cancellationTokenSource);
                await _textCanvasGroup.FadeToAsync(0f, animationDuration, _cancellationTokenSource);
            }
            catch {}
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}
