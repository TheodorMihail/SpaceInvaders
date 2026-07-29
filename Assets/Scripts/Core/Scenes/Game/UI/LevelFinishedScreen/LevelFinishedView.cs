using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/LevelFinishedScreenView")]
    public class LevelFinishedView : View
    {
        [Inject] private readonly ICustomFactory _factory;

        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private GameObject[] _starIcons;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private string _scoreString = "Score: {0}";

        [Header("Items Collected")]
        [SerializeField] private ItemSlotComponent _itemCellPrefab;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private TextMeshProUGUI _noItemsText;
        [SerializeField] private ItemTooltipComponent _tooltip;

        public event Action OnNextLevelButtonClicked;
        public event Action OnMainMenuButtonClicked;

        private void Awake()
        {
            _nextLevelButton.onClick.AddListener(() => OnNextLevelButtonClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuButtonClicked?.Invoke());
        }

        public void Initialize(bool allLevelsComplete, int starsEarned, int score,
            IEnumerable<(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity)> collectedItems)
        {
            _nextLevelButton.gameObject.SetActive(!allLevelsComplete);

            for (int i = 0; i < _starIcons.Length; i++)
            {
                _starIcons[i].SetActive(i < starsEarned);
            }

            _scoreText.text = string.Format(_scoreString, score);

            _tooltip.Hide();
            InitializeCollectedItems(collectedItems);
        }

        private void InitializeCollectedItems(IEnumerable<(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity)> collectedItems)
        {
            int count = 0;

            foreach ((InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity) item in collectedItems)
            {
                ItemSlotComponent cell = _factory.CreateFromPrefab(_itemCellPrefab, _itemsContainer);
                cell.SetItem(item.config, item.rarity);

                string instanceId = item.entry.InstanceId;
                cell.OnClicked += () => _tooltip.ShowReadOnly(cell.RectTransform, instanceId);

                count++;
            }

            if (_noItemsText != null)
            {
                _noItemsText.gameObject.SetActive(count == 0);
            }
        }
    }
}
