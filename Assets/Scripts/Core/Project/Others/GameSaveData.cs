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

    public class InventorySaveData : ISaveData
    {
        public const string SaveKey = "PlayerInventory";
        public List<InventoryItemEntry> Items = new();
    }

    public class EquipmentSaveData : ISaveData
    {
        public const string SaveKey = "PlayerEquipment";
        public List<EquippedSlotEntry> Slots = new();
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

    public class InventoryItemEntry
    {
        public string InstanceId;
        public string ItemId;
        public List<RolledAffixEntry> Affixes = new();
    }

    public class RolledAffixEntry
    {
        public string StatType;
        public float Bonus;
    }

    public class EquippedSlotEntry
    {
        public string Slot;
        public string InstanceId;
    }

    #endregion
}
