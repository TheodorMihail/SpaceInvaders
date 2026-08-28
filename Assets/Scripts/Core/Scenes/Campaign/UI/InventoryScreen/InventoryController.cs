using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.Campaign
{
    public class InventoryController : Controller<InventoryScreen, InventoryModel, InventoryView>
    {
        [Inject] private readonly IMessageBus _messageBus;

        public InventoryController(InventoryScreen uiComponent, InventoryModel model, InventoryView view)
            : base(uiComponent, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnItemClicked += OnItemClicked;
            _view.OnBackClicked += OnBackClicked;
            _messageBus.Subscribe<ItemEquipChangedMessage>(OnItemEquipChanged);
            _messageBus.Subscribe<ItemSoldMessage>(OnItemSold);

            _view.Setup();
            _view.RefreshStatsPanel(_model.GetStatsPanel());
        }

        public override void Dispose()
        {
            _view.OnItemClicked -= OnItemClicked;
            _view.OnBackClicked -= OnBackClicked;
            _messageBus.Unsubscribe<ItemEquipChangedMessage>(OnItemEquipChanged);
            _messageBus.Unsubscribe<ItemSoldMessage>(OnItemSold);
            base.Dispose();
        }

        private void OnItemClicked(RectTransform anchor, string instanceId)
        {
            _view.OpenTooltip(anchor, instanceId);
        }
        
        private void OnItemEquipChanged(ItemEquipChangedMessage message)
        {
            _view.ApplyEquipChange(message.EquippedInstanceId, message.UnequippedInstanceId);
            _view.RefreshStatsPanel(_model.GetStatsPanel());
        }

        private void OnItemSold(ItemSoldMessage message)
        {
            _view.RemoveItem(message.InstanceId);
            _view.RefreshStatsPanel(_model.GetStatsPanel());
            _view.RefreshCurrencyDisplay();
        }

        private void OnBackClicked()
        {
            Close();
        }
    }
}
