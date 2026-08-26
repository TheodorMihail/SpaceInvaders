using System.Threading;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Project
{
    /// <summary>
    /// Currency readout that counts up to each new balance rather than snapping. Shared by every
    /// screen showing currency, so the format is authored once.
    /// </summary>
    public class CurrencyUIComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private string _currencyString = "{0}";
        [SerializeField] private float _countDuration = 0.4f;

        private CancellationTokenSource _countCancellationTokenSource;
        private int _currentCurrency;

        /// <summary>Snaps to the balance, for a screen opening on a value the player never saw change.</summary>
        public void Initialize(int currency)
        {
            CancelCount();
            FormatCurrency(currency);
        }

        private void OnDestroy()
        {
            CancelCount();
        }

        /// <summary>Counts in either direction, for spending as well as earning.</summary>
        public async void UpdateCurrency(int currency)
        {
            CancelCount();
            _countCancellationTokenSource = new CancellationTokenSource();

            await _currencyText.CountdownAsync(_currentCurrency, currency, _countDuration, FormatCurrency,
                _countCancellationTokenSource);

            FormatCurrency(currency);
        }

        /// <summary>Also the count's per-frame callback, so the tracked balance always matches what is
        /// on screen. A count cancelled halfway then resumes from where the eye left it.</summary>
        private void FormatCurrency(int currency)
        {
            _currentCurrency = currency;
            _currencyText.text = string.Format(_currencyString, currency);
        }

        private void CancelCount()
        {
            _countCancellationTokenSource?.CancelAndDispose();
            _countCancellationTokenSource = null;
        }
    }
}
