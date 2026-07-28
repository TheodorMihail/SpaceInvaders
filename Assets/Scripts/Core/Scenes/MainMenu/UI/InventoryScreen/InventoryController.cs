using BaseArchitecture.Core;
using SpaceInvaders.Project;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
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

            _view.Setup();
            _view.RefreshStatsPanel(_model.GetStatsPanel());
        }

        public override void Dispose()
        {
            _view.OnItemClicked -= OnItemClicked;
            _view.OnBackClicked -= OnBackClicked;
            _messageBus.Unsubscribe<ItemEquipChangedMessage>(OnItemEquipChanged);
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

        private void OnBackClicked()
        {
            Close();
        }
    }
}
