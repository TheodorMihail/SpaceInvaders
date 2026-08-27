using System;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceInvaders.Scenes.Game
{
    [AddressablePath("Screens/GameOverScreenView")]
    public class GameOverView : View
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TextMeshProUGUI _scoreText;

        [SerializeField] private string _scoreString = "Score: {0}";

        public event Action OnRestartButtonClicked;
        public event Action OnMainMenuButtonClicked;

        public void Initialize(GameOverOptionTypes options, int score)
        {
            _restartButton.gameObject.SetActive(options.HasFlag(GameOverOptionTypes.Restart));
            _mainMenuButton.gameObject.SetActive(options.HasFlag(GameOverOptionTypes.MainMenu));

            _scoreText.text = string.Format(_scoreString, score);
        }

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => OnRestartButtonClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuButtonClicked?.Invoke());
        }
    }
}
