using System;
using SpaceInvaders.Project;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    /// <summary>A container that holds one item or none - shared by ship slots and grid cells.</summary>
    public class ItemSlotComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityFrameImage;
        [SerializeField] private GameObject _selectedHighlight;
        [SerializeField] private Button _button;

        [Header("Optional")]
        [SerializeField] private GameObject _equippedBadge;

        private bool _hasItem;
        private Sprite _defaultIcon;

        public RectTransform RectTransform => (RectTransform)transform;

        public event Action OnClicked;

        private void Awake()
        {
            _defaultIcon = _iconImage.sprite;
            _button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void SetItem(ItemConfigSO config, ItemRarityConfigSO rarityConfig)
        {
            _hasItem = true;
            _iconImage.sprite = config.Icon;
            _rarityFrameImage.color = rarityConfig != null ? rarityConfig.DisplayColor : Color.white;
            _button.interactable = true;
        }

        public void RemoveItem()
        {
            _hasItem = false;
            _iconImage.sprite = _defaultIcon;
            _rarityFrameImage.color = Color.white;
            _button.interactable = false;
            SetEquipped(false);
            SetSelected(false);
        }

        // Guarded by _hasItem so a caller can never leave an empty slot looking selected/equipped.
        public void SetEquipped(bool isEquipped)
        {
            if (_equippedBadge != null)
            {
                _equippedBadge.SetActive(_hasItem && isEquipped);
            }
        }

        public void SetSelected(bool isSelected)
        {
            _selectedHighlight.SetActive(_hasItem && isSelected);
        }
    }
}
