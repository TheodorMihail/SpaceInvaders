using System.Collections.Generic;
using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    // One save blob per manager, each with its own SaveKey. Enums are stored as strings: renaming an
    // enum member invalidates existing saves, reordering does not.
    //
    // CurrentVersion starts at 0 and is bumped only when stored data stops being usable, which
    // discards that blob on the next load. Saves written before versioning existed read as 0, so
    // blobs that were never invalidated are kept.

    #region  SaveData

    public class LevelsSaveData : IVersionedSaveData
    {
        public const string SaveKey = "LevelsProgress";
        public const int CurrentVersion = 0;

        public int Version { get; set; }
        public List<LevelSaveEntry> Levels = new();
    }

    public class TalentsSaveData : IVersionedSaveData
    {
        public const string SaveKey = "TalentsProgress";
        public const int CurrentVersion = 0;

        public int Version { get; set; }
        public List<TalentSaveEntry> Talents = new();
    }

    public class CurrencySaveData : IVersionedSaveData
    {
        public const string SaveKey = "PlayerCurrency";
        public const int CurrentVersion = 0;

        public int Version { get; set; }
        public int Amount;
    }

    public class InventorySaveData : IVersionedSaveData
    {
        public const string SaveKey = "PlayerInventory";

        /// <summary>1: the item catalogue was rebuilt and every shipped item id was replaced.</summary>
        public const int CurrentVersion = 1;

        public int Version { get; set; }
        public List<InventoryItemEntry> Items = new();
    }

    public class EquipmentSaveData : IVersionedSaveData
    {
        public const string SaveKey = "PlayerEquipment";

        /// <summary>1: the slots referenced items that version 1 of the inventory discarded.</summary>
        public const int CurrentVersion = 1;

        public int Version { get; set; }
        public List<EquippedSlotEntry> Slots = new();
    }

    /// <summary>Lives in the Expedition profile, so a run and the Campaign save never mix. Holds only
    /// what is the run's own: items, talents and scrap sit in the usual managers against that same
    /// profile.</summary>
    public class ExpeditionRunSaveData : IVersionedSaveData
    {
        public const string SaveKey = "ExpeditionRun";
        public const int CurrentVersion = 0;

        public int Version { get; set; }
        public string RunPhase;
        public int Seed;
        public int CurrentNodeId;
        public float RemainingHealthRatio;
        public int ShopRerollsUsed;
        public List<ExpeditionNodeEntry> Nodes = new();
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
        public List<AffixEntry> Affixes = new();
    }

    public class AffixEntry
    {
        public string StatType;
        public string ValueType;
        public float Bonus;
    }

    public class EquippedSlotEntry
    {
        public string Slot;
        public string InstanceId;
    }

    public class ExpeditionNodeEntry
    {
        public int Id;
        public int Depth;
        public int Column;
        public string NodeType;
        public string State;

        /// <summary>LevelConfigSO.ObjectID. Empty on nodes that are not played.</summary>
        public string LevelId;
        public List<int> NextNodeIds = new();
    }

    #endregion
}
