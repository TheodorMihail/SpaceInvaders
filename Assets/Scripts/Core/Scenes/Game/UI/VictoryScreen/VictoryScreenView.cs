using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/VictoryScreenView")]
    public class VictoryScreenView : View
    {
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private GameObject[] _starIcons;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private string _scoreString = "Score: {0}";

        [Header("Items Collected")]
        [SerializeField] private ItemsContainerUIComponent _itemsContainer;
        [SerializeField] private ItemTooltipComponent _tooltip;

        public event Action OnNextLevelButtonClicked;
        public event Action OnRetryButtonClicked;
        public event Action OnMainMenuButtonClicked;

        public void Initialize(GameOverOptionTypes options, int starsEarned, int score,
            IEnumerable<(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity)> collectedItems)
        {
            _nextLevelButton.gameObject.SetActive(options.HasFlag(GameOverOptionTypes.NextLevel));
            _retryButton.gameObject.SetActive(options.HasFlag(GameOverOptionTypes.Retry));
            _mainMenuButton.gameObject.SetActive(options.HasFlag(GameOverOptionTypes.MainMenu));

            for (int i = 0; i < _starIcons.Length; i++)
            {
                _starIcons[i].SetActive(i < starsEarned);
            }

            _scoreText.text = string.Format(_scoreString, score);

            _tooltip.Hide();
            InitializeCollectedItems(collectedItems);
        }

        private void Awake()
        {
            _nextLevelButton.onClick.AddListener(() => OnNextLevelButtonClicked?.Invoke());
            _retryButton.onClick.AddListener(() => OnRetryButtonClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuButtonClicked?.Invoke());
            _itemsContainer.OnItemClicked += OnItemClicked;
        }

        private void OnDestroy()
        {
            _itemsContainer.OnItemClicked -= OnItemClicked;
        }

        private void InitializeCollectedItems(IEnumerable<(InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity)> collectedItems)
        {
            _itemsContainer.Clear();

            foreach ((InventoryItemEntry entry, ItemConfigSO config, ItemRarityConfigSO rarity) item in collectedItems)
            {
                _itemsContainer.AddItem(item.entry, item.config, item.rarity);
            }
        }

        /// <summary>Read-only here: the run is over, so loot is shown rather than managed.</summary>
        private void OnItemClicked(RectTransform anchor, string instanceId)
        {
            _tooltip.ShowReadOnly(anchor, instanceId);
        }
    }
}
