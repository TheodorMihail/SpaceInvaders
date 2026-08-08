using System.Collections.Generic;
using System.Linq;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;

namespace SpaceInvaders.Project
{
    public interface IPowerupsRepository
    {
        bool TryGetPowerupConfig(PowerupTypes powerupType, out PowerupConfigSO config);
        IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs();
    }

    public class PowerupsRepository : Repository, IPowerupsRepository
    {
        public PowerupsRepository(PowerupsDataConfigSO powerupsDataConfigSO)
        {
            AddObjects(powerupsDataConfigSO.PowerupConfigs);
            AddObject(powerupsDataConfigSO);
        }

        public bool TryGetPowerupConfig(PowerupTypes powerupType, out PowerupConfigSO config)
        {
            return TryGet(powerupType.ToString(), out config);
        }

        public IReadOnlyList<PowerupConfigSO> GetAllPowerupConfigs()
        {
            return GetAll<PowerupConfigSO>().ToArray();
        }
    }
}
