using System;
using SpaceInvaders.Project;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class InventoryItemComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityFrameImage;
        [SerializeField] private GameObject _equippedBadge;
        [SerializeField] private GameObject _selectedHighlight;
        [SerializeField] private Button _itemButton;

        private string _instanceId;

        public string InstanceId => _instanceId;
        public RectTransform RectTransform => (RectTransform)transform;

        public event Action<string> OnItemClicked;

        private void Awake()
        {
            _itemButton.onClick.AddListener(() => OnItemClicked?.Invoke(_instanceId));
        }

        public void Setup(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarityConfig, bool isEquipped, bool isSelected)
        {
            _instanceId = entry.InstanceId;
            _iconImage.sprite = config.Icon;
            _equippedBadge.SetActive(isEquipped);
            _selectedHighlight.SetActive(isSelected);
            _rarityFrameImage.color = rarityConfig != null ? rarityConfig.DisplayColor : Color.white;
        }
    }
}
