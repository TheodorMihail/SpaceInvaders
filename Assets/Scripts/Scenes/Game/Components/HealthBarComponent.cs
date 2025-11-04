using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    public class HealthBarComponent : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _healthBarCanvasGroup;
        [SerializeField] private Image _healthFillImage;

        private int _maxHealth;

        public void Initialize(int currentHealth, int maxHealth)
        {
            _maxHealth = maxHealth;
            UpdateHealth(currentHealth);
        }

        public void UpdateHealth(int currentHealth)
        {
            _healthBarCanvasGroup.alpha = currentHealth < _maxHealth ? 1f : 0f;
            float fillAmount = (float)currentHealth / _maxHealth;
            _healthFillImage.fillAmount = fillAmount;
        }
    }
}
