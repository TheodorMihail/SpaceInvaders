using BaseArchitecture.Core;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Game;
using Zenject;

namespace SpaceInvaders.Scenes.MainMenu
{
    public class TalentTreeController : Controller<TalentTreeScreen, TalentTreeModel, TalentTreeView>
    {
        [Inject] private readonly IRepositoryManager _repository;
        [Inject] private readonly ITalentManager _talentManager;

        public TalentTreeController(TalentTreeScreen uiComponent, TalentTreeModel model, TalentTreeView view)
            : base(uiComponent, model, view)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            _view.OnTalentPurchaseClicked += OnTalentPurchaseClicked;
            _view.OnBackClicked += OnBackClicked;
            _view.SetupTalents(_repository.GetAllTalentConfigs());
        }

        public override void Dispose()
        {
            _view.OnTalentPurchaseClicked -= OnTalentPurchaseClicked;
            _view.OnBackClicked -= OnBackClicked;
            base.Dispose();
        }

        private void OnTalentPurchaseClicked(TalentTypes type)
        {
            if (_talentManager.TryPurchaseLevel(type))
            {
                _view.RefreshAllTalentButtons();
            }
        }

        private void OnBackClicked()
        {
            Close();
        }
    }
}
