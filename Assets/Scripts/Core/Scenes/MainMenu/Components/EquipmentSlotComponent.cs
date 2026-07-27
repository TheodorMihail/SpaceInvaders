using System;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class EquipmentSlotComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityFrameImage;
        [SerializeField] private TextMeshProUGUI _slotNameText;
        [SerializeField] private GameObject _selectedHighlight;
        [SerializeField] private Button _slotButton;

        [Header("Colors")]
        [SerializeField] private Color _emptyFrameColor = Color.gray;

        private EquipmentSlots _slot;

        public RectTransform RectTransform => (RectTransform)transform;

        public event Action<EquipmentSlots> OnSlotClicked;

        private void Awake()
        {
            _slotButton.onClick.AddListener(() => OnSlotClicked?.Invoke(_slot));
        }

        public void Setup(EquipmentSlotConfigDTO slotConfig, ItemConfigSO equippedConfig, ItemRarityConfigSO rarityConfig, bool isSelected)
        {
            _slot = slotConfig.Slot;
            _slotNameText.text = slotConfig.DisplayName;
            _selectedHighlight.SetActive(isSelected);

            bool hasItem = equippedConfig != null;
            _iconImage.enabled = hasItem;
            _slotButton.interactable = hasItem; // empty slots aren't clickable - nothing to select or act on

            if (hasItem)
            {
                _iconImage.sprite = equippedConfig.Icon;
                _rarityFrameImage.color = rarityConfig != null ? rarityConfig.DisplayColor : _emptyFrameColor;
            }
            else
            {
                _rarityFrameImage.color = _emptyFrameColor;
            }
        }
    }
}
