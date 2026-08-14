using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Project;
using Zenject;
using static SpaceInvaders.Scenes.Game.GameplayHUD;

namespace SpaceInvaders.Scenes.Game
{
    public class GameplayHUDModel : Model, IModelWithParams<GameplayHUDParams>
    {
        [Inject] private readonly IPlayerManager _playerManager;

        public int Score { get; set; } = 0;
        public int LevelNumber { get; set; } = 0;
        public int WaveNumber { get; set; } = 0;
        public int TotalWaves { get; set; } = 0;
        public float CritIndicatorDuration { get; set; } = 0.75f;

        /// <summary>Collected this run, per rarity. Rarities absent from it have not dropped yet.</summary>
        public Dictionary<ItemRarityTypes, int> LootCounts { get; set; } = new();

        public void InitializeWithParameters(GameplayHUDParams parameters)
        {
            LevelNumber = parameters.LevelNumber;
        }

        /// <summary>The HUD is created after the player spawns, and the bus has no replay, so the
        /// starting ammo has to be read directly. False means the ammo display stays hidden.</summary>
        public bool TryGetAmmo(out int currentAmmo, out int maxAmmo)
        {
            currentAmmo = 0;
            maxAmmo = 0;

            ShipStats stats = _playerManager.PlayerStats;
            if (stats == null || stats.HasUnlimitedAmmo)
            {
                return false;
            }

            currentAmmo = stats.CurrentAmmo;
            maxAmmo = stats.CurrentMaxAmmo;

            return true;
        }

        public int IncrementLootCount(ItemRarityTypes rarity)
        {
            LootCounts.TryGetValue(rarity, out int count);
            count++;
            LootCounts[rarity] = count;

            return count;
        }
    }
}
