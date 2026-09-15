using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    [CreateAssetMenu(fileName = "ExpeditionRunDataConfig", menuName = "SpaceInvaders/Game Modes/Expedition Run Data Config")]
    public class ExpeditionRunDataConfigSO : GameModeRunDataConfigSO
    {
        public override GameModeTypes Mode => GameModeTypes.Expedition;
    }
}
