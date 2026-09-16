using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    [AddressablePath("Screens/ExpeditionRewardScreenView")]
    public class ExpeditionRewardScreenView : View
    {
        [SerializeField] private ExpeditionTalentOfferUIComponent _offer;

        public event Action<string> OnTalentClicked;

        public void Initialize(IEnumerable<ExpeditionTalentOfferDTO> offers)
        {
            _offer.Build(offers);
        }

        private void Awake()
        {
            _offer.OnTalentClicked += HandleTalentClicked;
        }

        private void OnDestroy()
        {
            _offer.OnTalentClicked -= HandleTalentClicked;
        }

        private void HandleTalentClicked(string talentId)
        {
            OnTalentClicked?.Invoke(talentId);
        }
    }
}
