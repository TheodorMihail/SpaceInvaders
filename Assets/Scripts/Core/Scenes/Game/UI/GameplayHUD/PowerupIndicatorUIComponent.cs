using System.Threading;
using BaseArchitecture.Core;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    public class PowerupIndicatorUIComponent : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _countdownText;

        private CancellationTokenSource _cts;

        public void Initialize(Sprite icon, float duration)
        {
            _icon.sprite = icon;
            _cts?.CancelAndDispose();

            if (duration <= 0f)
            {
                _countdownText.text = string.Empty;
                return;
            }

            _cts = new CancellationTokenSource();
            RunCountdown(duration, _cts).Forget();
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            _cts?.CancelAndDispose();
            _cts = null;
        }

        private async UniTaskVoid RunCountdown(float duration, CancellationTokenSource cts)
        {
            await _countdownText.CountdownAsync(duration, 0, duration, null, cts);
        }
    }
}
