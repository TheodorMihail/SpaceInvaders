using TMPro;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>How far through the level's waves the run is. Hidden until the first wave starts,
    /// since there is nothing meaningful to show before then.</summary>
    public class WaveUIComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private string _waveString = "Wave: {0}/{1}";

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public void SetWave(int waveNumber, int totalWaves)
        {
            Show(true);
            _waveText.text = string.Format(_waveString, waveNumber, totalWaves);
        }
    }
}
