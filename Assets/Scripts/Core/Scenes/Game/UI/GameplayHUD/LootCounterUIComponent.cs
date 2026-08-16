using BaseArchitecture.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>How many items of one rarity the run has collected. Spawned on the first pickup of
    /// that rarity, so tiers the player has not found yet stay off the HUD.</summary>
    public class LootCounterUIComponent : MonoBehaviour, IPoolableObject
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;

        [SerializeField] private string _countString = "{0}";

        public void Initialize(Sprite icon, int count)
        {
            _iconImage.sprite = icon;
            SetCount(count);
        }

        public void SetCount(int count)
        {
            _countText.text = string.Format(_countString, count);
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
        }
    }
}
