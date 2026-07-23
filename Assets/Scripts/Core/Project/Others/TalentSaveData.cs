using System.Collections.Generic;

namespace SpaceInvaders.Project
{
    public class TalentSaveEntry
    {
        public string TalentType;
        public int Level;
    }

    public class TalentSaveData
    {
        public List<TalentSaveEntry> Talents = new();
    }
}
