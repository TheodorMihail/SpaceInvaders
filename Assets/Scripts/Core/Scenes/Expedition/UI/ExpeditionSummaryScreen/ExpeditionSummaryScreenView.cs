using System;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Expedition
{
    [AddressablePath("Screens/ExpeditionSummaryScreenView")]
    public class ExpeditionSummaryScreenView : View
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _resultText;

        [Header("Strings")]
        [SerializeField] private string _completedString = "EXPEDITION COMPLETE";
        [SerializeField] private string _endedString = "EXPEDITION ENDED - LEVEL {0}";

        public event Action OnContinueButtonClicked;

        /// <summary>How far a run got only matters when it did not get all the way.</summary>
        public void Initialize(ExpeditionRunResultTypes result, int depthReached)
        {
            _resultText.text = result == ExpeditionRunResultTypes.Completed
                ? _completedString
                : string.Format(_endedString, depthReached);
        }

        private void Awake()
        {
            _continueButton.onClick.AddListener(() => OnContinueButtonClicked?.Invoke());
        }
    }
}
