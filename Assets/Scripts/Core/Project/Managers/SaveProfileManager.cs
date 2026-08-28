using System.Collections.Generic;
using System.IO;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;

namespace SpaceInvaders.Project
{
    public interface ISaveProfileManager
    {
        /// <summary>The save file a mode stores everything in. Every mode has one, so never null.</summary>
        IPersistenceManager GetProfile(GameModeTypes mode);
        IPersistenceManager GetGeneralProfile();
    }

    /// <summary>
    /// One save file per game mode, so no two modes ever share stored state. Save keys are identical
    /// across profiles, since the file is what separates them, and a mode's whole persisted world can
    /// be reasoned about as one file. Settings that belong to no mode use the general profile, bound
    /// as the plain persistence manager.
    /// </summary>
    public class SaveProfileManager : ISaveProfileManager
    {
        /// <summary>Named after the mode, so adding one needs no change here.</summary>
        private const string FileNameFormat = "{0}Save.json";

        public const string GeneralProfileName = "General";

        /// <summary>Created on first use and kept: each instance caches its file's contents, so a
        /// second one over the same path would silently overwrite the first's writes.</summary>
        private readonly Dictionary<string, IPersistenceManager> _profiles = new();

        public IPersistenceManager GetProfile(GameModeTypes mode)
        {
            return GetOrCreateProfile(mode.ToString());
        }

        public IPersistenceManager GetGeneralProfile()
        {
            return GetOrCreateProfile(GeneralProfileName);
        }

        /// <remarks>A mode's file is named after its enum member, so renaming one orphans its save the
        /// same way renaming a persisted enum value does. Reordering is safe.</remarks>
        public static string GetFilePath(string profileName)
        {
            return Path.Combine(Application.persistentDataPath, string.Format(FileNameFormat, profileName));
        }
        

        private IPersistenceManager GetOrCreateProfile(string profileName)
        {
            if (!_profiles.TryGetValue(profileName, out IPersistenceManager profile))
            {
                profile = new PersistenceManager(GetFilePath(profileName));
                _profiles[profileName] = profile;
            }

            return profile;
        }
    }
}
