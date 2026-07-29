using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public interface IProjectRepository
    {
        ProjectDataConfigSO GetProjectDataConfig();
    }

    public class ProjectRepository : Repository, IProjectRepository
    {
        public ProjectRepository(ProjectDataConfigSO projectDataConfigSO)
        {
            AddObject(projectDataConfigSO);
        }

        public ProjectDataConfigSO GetProjectDataConfig()
        {
            TryGet(nameof(ProjectDataConfigSO), out ProjectDataConfigSO config);
            return config;
        }
    }
}
