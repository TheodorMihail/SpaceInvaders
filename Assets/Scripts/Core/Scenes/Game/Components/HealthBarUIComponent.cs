using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    public class HealthBarUIComponent : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _healthBarCanvasGroup;
        [SerializeField] private Image _healthFillImage;

        private int _maxHealth;

        public void Initialize(int currentHealth, int maxHealth, bool autoHide = true)
        {
            _maxHealth = maxHealth;
            UpdateHealth(currentHealth, autoHide);
        }

        public void UpdateHealth(int currentHealth, bool autoHide = true)
        {
            _healthBarCanvasGroup.alpha = autoHide ? (currentHealth < _maxHealth ? 1f : 0f) : 1f;
            float fillAmount = (float)currentHealth / _maxHealth;
            _healthFillImage.fillAmount = fillAmount;
        }
    }
}
