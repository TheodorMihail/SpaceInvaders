using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public interface IProjectRepository
    {
        ProjectDataConfigSO GetProjectDataConfig();

        /// <summary>What the cheats hand out. Served here rather than from a repository of its own,
        /// since it is one more project-wide config.</summary>
        DebugDataConfigSO GetDebugDataConfig();
    }

    public class ProjectRepository : Repository, IProjectRepository
    {
        public ProjectRepository(ProjectDataConfigSO projectDataConfigSO, DebugDataConfigSO debugDataConfigSO)
        {
            AddObject(projectDataConfigSO);
            AddObject(debugDataConfigSO);
        }

        public ProjectDataConfigSO GetProjectDataConfig()
        {
            TryGet(nameof(ProjectDataConfigSO), out ProjectDataConfigSO config);
            return config;
        }

        public DebugDataConfigSO GetDebugDataConfig()
        {
            TryGet(nameof(DebugDataConfigSO), out DebugDataConfigSO config);
            return config;
        }
    }
}
