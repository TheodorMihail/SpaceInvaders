using BaseArchitecture.Core;

namespace SpaceInvaders.Scenes.Game
{
    public class GameAnnouncerHUDModel : Model
    {
        public float AnimationDurationSeconds { get; } = 1f;
        public string BossWaveText { get; } = "BOSS WARNING!";

        public string LevelTextFormat(int number)
        {
            return $"Level {number}";
        }

        public string NormalWaveTextFormat(int number)
        {
            return $"Wave {number}";
        }
    }
}
