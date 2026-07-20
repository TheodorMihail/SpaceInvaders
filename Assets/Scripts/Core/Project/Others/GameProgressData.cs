using System.Collections.Generic;

namespace SpaceInvaders.Project
{
    public class LevelProgressEntry
    {
        public int LevelIndex;
        public int Stars;
        public bool Unlocked;
    }

    public class GameProgressData
    {
        public List<LevelProgressEntry> Levels = new();
    }
}
