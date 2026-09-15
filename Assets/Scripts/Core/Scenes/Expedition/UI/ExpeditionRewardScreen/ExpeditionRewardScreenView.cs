using System;
using System.Collections.Generic;
using BaseArchitecture.Core;
using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Expedition
{
    [AddressablePath("Screens/ExpeditionRewardScreenView")]
    public class ExpeditionRewardScreenView : View
    {
        [SerializeField] private ExpeditionPerkOfferUIComponent _offer;


        public event Action<string> OnPerkClicked;

        public void Initialize(IEnumerable<(ExpeditionPerkConfigSO perk, ExpeditionPerkRarityConfigSO rarity)> choices)
        {
            _offer.Build(choices);
        }

        private void Awake()
        {
            _offer.OnPerkClicked += HandlePerkClicked;
        }

        private void OnDestroy()
        {
            _offer.OnPerkClicked -= HandlePerkClicked;
        }

        private void HandlePerkClicked(string perkId)
        {
            OnPerkClicked?.Invoke(perkId);
        }
    }
}
