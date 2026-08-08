using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public interface ISoundsRepository
    {
        bool TryGetSoundConfig(SoundTypes soundType, out SoundConfigSO config);
    }

    public class SoundsRepository : Repository, ISoundsRepository
    {
        public SoundsRepository(SoundsDataConfigSO soundsDataConfigSO)
        {
            AddObjects(soundsDataConfigSO.SoundConfigs);
            AddObject(soundsDataConfigSO);
        }

        public bool TryGetSoundConfig(SoundTypes soundType, out SoundConfigSO config)
        {
            return TryGet(soundType.ToString(), out config);
        }
    }
}
