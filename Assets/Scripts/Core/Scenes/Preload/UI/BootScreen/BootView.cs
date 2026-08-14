using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Preload
{
    [AddressablePath("Screens/BootScreenView")]
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

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }

        public async UniTask PlayLoadingAnimation(float duration, float endDelay)
        {
            await _loadingText.CountdownAsync(0, 100, duration, (val) =>
            {
                _loadingText.text = string.Format(_loadingString, val);

            }, _cancellationTokenSource);
            
            _loadingText.text = _loadingFinishedString;

            //Adding another delay to see the loading finished text
            await UniTask.Delay((int)endDelay * 1000, cancellationToken: _cancellationTokenSource.Token);
        }
    }
}