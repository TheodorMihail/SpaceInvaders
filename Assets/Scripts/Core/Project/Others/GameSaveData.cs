using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    #region  SaveData

    public class LevelsSaveData : ISaveData
    {
        public const string SaveKey = "LevelsProgress";
        public List<LevelSaveEntry> Levels = new();
    }

    public class TalentsSaveData : ISaveData
    {
        public const string SaveKey = "TalentsProgress";
        public List<TalentSaveEntry> Talents = new();
    }

    public class CurrencySaveData : ISaveData
    {
        public const string SaveKey = "PlayerCurrency";
        public int Amount;
    }

    #endregion

    #region  SaveEntries

    public class LevelSaveEntry
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

    #endregion
}
