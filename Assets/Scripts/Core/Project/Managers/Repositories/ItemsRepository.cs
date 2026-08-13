using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IItemsRepository
    {
        bool TryGetItemConfig(string itemId, out ItemConfigSO config);
        IReadOnlyList<ItemConfigSO> GetAllItemConfigs();
        bool TryGetItemRarityConfig(ItemRarityTypes rarity, out ItemRarityConfigSO config);
        IReadOnlyList<ItemRarityConfigSO> GetAllItemRarityConfigs();
        IReadOnlyList<EquipmentSlotConfigDTO> GetAllEquipmentSlotConfigs();
        ItemPickupBehaviourComponent GetItemPickupPrefab();
    }

    public class ItemsRepository : Repository, IItemsRepository
    {
        public ItemsRepository(ItemsDataConfigSO itemsDataConfigSO)
        {
            AddObjects(itemsDataConfigSO.ItemConfigs);
            AddObjects(itemsDataConfigSO.RarityConfigs);
            AddObject(itemsDataConfigSO);
        }

        public bool TryGetItemConfig(string itemId, out ItemConfigSO config)
        {
            return TryGet(itemId, out config);
        }

        public IReadOnlyList<ItemConfigSO> GetAllItemConfigs()
        {
            return GetAll<ItemConfigSO>().ToArray();
        }

        public bool TryGetItemRarityConfig(ItemRarityTypes rarity, out ItemRarityConfigSO config)
        {
            return TryGet(rarity.ToString(), out config);
        }

        public IReadOnlyList<ItemRarityConfigSO> GetAllItemRarityConfigs()
        {
            return GetAll<ItemRarityConfigSO>().ToArray();
        }

        public IReadOnlyList<EquipmentSlotConfigDTO> GetAllEquipmentSlotConfigs()
        {
            TryGet(nameof(ItemsDataConfigSO), out ItemsDataConfigSO config);
            return config.SlotConfigs;
        }

        public ItemPickupBehaviourComponent GetItemPickupPrefab()
        {
            TryGet(nameof(ItemsDataConfigSO), out ItemsDataConfigSO config);
            return config.ItemPickupPrefab;
        }
    }
}
