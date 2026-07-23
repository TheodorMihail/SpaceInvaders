using System.Collections.Generic;

namespace SpaceInvaders.Project
{
    public class LevelProgressEntry
    {
        public int LevelIndex;
        public int Stars;
        public bool Unlocked;
    }

    public class TalentSaveEntry
    {
        public string TalentType;
        public int Level;
    }

    public class GameProgressData
    {
        public List<LevelProgressEntry> Levels = new();
        public int Currency;
        public List<TalentSaveEntry> Talents = new();
    }
}
