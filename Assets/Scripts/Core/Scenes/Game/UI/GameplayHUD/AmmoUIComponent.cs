using System.Threading;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Ammo readout that swaps to a reload icon and counts the remaining seconds down.</summary>
    public class AmmoUIComponent : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _ammoText;

        [SerializeField] private Sprite _ammoSprite;
        [SerializeField] private Sprite _reloadingSprite;

        [SerializeField] private string _ammoString = "{0}/{1}";

        [Tooltip("Scaled up: the glyph is a lone short symbol where the count fills the whole box.")]
        [SerializeField] private string _unlimitedAmmoString = "<size=200%>∞</size>";

        private CancellationTokenSource _reloadCancellationTokenSource;

        private int _currentAmmo;
        private int _maxAmmo;
        private bool _hasUnlimitedAmmo;

        private void OnDestroy()
        {
            CancelReloadCountdown();
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public void UpdateAmmo(int currentAmmo, int maxAmmo)
        {
            _currentAmmo = currentAmmo;
            _maxAmmo = maxAmmo;

            RefreshAmmo();
        }

        /// <summary>Pins the readout to the unlimited sign, so the rounds still reported underneath
        /// never show through while ammo does not matter.</summary>
        public void SetUnlimitedAmmo(bool hasUnlimitedAmmo)
        {
            _hasUnlimitedAmmo = hasUnlimitedAmmo;

            RefreshAmmo();
        }

        /// <summary>The countdown replaces the ammo text until the magazine is refilled.</summary>
        public async void ShowReloading(float duration)
        {
            CancelReloadCountdown();
            _reloadCancellationTokenSource = new CancellationTokenSource();

            _iconImage.sprite = _reloadingSprite;

            await _ammoText.CountdownAsync(duration, 0, duration, null, _reloadCancellationTokenSource);
        }

        private void RefreshAmmo()
        {
            CancelReloadCountdown();

            _iconImage.sprite = _ammoSprite;
            _ammoText.text = _hasUnlimitedAmmo
                ? _unlimitedAmmoString
                : string.Format(_ammoString, _currentAmmo, _maxAmmo);
        }

        private void CancelReloadCountdown()
        {
            _reloadCancellationTokenSource?.CancelAndDispose();
            _reloadCancellationTokenSource = null;
        }
    }
}
