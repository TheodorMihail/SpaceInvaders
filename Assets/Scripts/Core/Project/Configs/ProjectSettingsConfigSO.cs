using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    [CreateAssetMenu(fileName = "ProjectDataConfig", menuName = "SpaceInvaders/Data Config/Project Data Config")]
    public class ProjectDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Editor Testing")]
        [SerializeField] private bool _editorForceTouchPlatform;

        public virtual bool EditorForceTouchPlatform => _editorForceTouchPlatform;

        public string ObjectID => nameof(ProjectDataConfigSO);
    }
}
