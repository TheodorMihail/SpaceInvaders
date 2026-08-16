using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>The level number the run is currently playing.</summary>
    public class LevelUIComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private string _levelString = "Level: {0}";

        public void SetLevel(int levelNumber)
        {
            _levelText.text = string.Format(_levelString, levelNumber);
        }
    }
}
