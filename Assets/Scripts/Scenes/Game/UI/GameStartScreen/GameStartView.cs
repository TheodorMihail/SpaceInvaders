using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/GameStartView")]
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

        public async UniTask StartCountdownAnimation(int countdownValue)
        {
            _countdownText.gameObject.SetActive(true);
            _pressAnyKeyText.gameObject.SetActive(false);
            _countdownText.text = countdownValue.ToString();

            var tween = DOTween.To(() => countdownValue, x => countdownValue = x, 0, countdownValue)
                .OnUpdate(() =>
                {
                    if (Mathf.RoundToInt(countdownValue) > 0)
                    {
                        _countdownText.text = Mathf.RoundToInt(countdownValue).ToString();
                    }
                    else
                    {
                        _countdownText.text = _startString;
                    }
                });

            await tween.ToUniTask(cancellationToken: _cancellationTokenSource.Token);
        }
        
        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}
