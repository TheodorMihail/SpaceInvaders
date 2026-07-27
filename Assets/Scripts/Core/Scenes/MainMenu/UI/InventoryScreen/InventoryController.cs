using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class InventoryController : Controller<InventoryScreen, InventoryModel, InventoryView>
    {
        [Inject] private readonly IEquipmentManager _equipmentManager;

        public InventoryController(InventoryScreen uiComponent, InventoryModel model, InventoryView view)
            : base(uiComponent, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnSlotClicked += OnSlotClicked;
            _view.OnItemClicked += OnItemClicked;
            _view.OnTooltipActionClicked += OnTooltipActionClicked;
            _view.OnTooltipCloseClicked += OnTooltipCloseClicked;
            _view.OnBackClicked += OnBackClicked;
            _view.Setup(_model);
        }

        public override void Dispose()
        {
            _view.OnSlotClicked -= OnSlotClicked;
            _view.OnItemClicked -= OnItemClicked;
            _view.OnTooltipActionClicked -= OnTooltipActionClicked;
            _view.OnTooltipCloseClicked -= OnTooltipCloseClicked;
            _view.OnBackClicked -= OnBackClicked;
            base.Dispose();
        }

        /// <summary>Clicking an already-open slot's tooltip closes it; otherwise opens it.</summary>
        private void OnSlotClicked(EquipmentSlots slot)
        {
            if (_model.OpenSlot == slot)
            {
                _model.CloseTooltip();
            }
            else
            {
                _model.OpenItemInstanceId = null;
                _model.OpenSlot = slot;
            }

            _view.Refresh();
        }

        private void OnItemClicked(string instanceId)
        {
            if (_model.OpenItemInstanceId == instanceId)
            {
                _model.CloseTooltip();
            }
            else
            {
                _model.OpenSlot = null;
                _model.OpenItemInstanceId = instanceId;
            }

            _view.Refresh();
        }

        /// <summary>Equip when an item's tooltip is open, unequip when a filled slot's tooltip is open.</summary>
        private void OnTooltipActionClicked()
        {
            if (_model.OpenItemInstanceId != null)
            {
                _equipmentManager.Equip(_model.OpenItemInstanceId);
            }
            else if (_model.OpenSlot != null)
            {
                _equipmentManager.Unequip(_model.OpenSlot.Value);
            }

            _model.CloseTooltip();
            _view.Refresh();
        }

        private void OnTooltipCloseClicked()
        {
            _model.CloseTooltip();
            _view.Refresh();
        }

        private void OnBackClicked()
        {
            Close();
        }
    }
}
