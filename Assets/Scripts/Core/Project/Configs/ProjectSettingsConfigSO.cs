using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Project
{
    [CreateAssetMenu(fileName = "ProjectDataConfig", menuName = "SpaceInvaders/Data Config/Project Data Config")]
    public class ProjectDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Editor Testing")]
        [SerializeField] private bool _editorForceTouchPlatform;
        [SerializeField] private int _debugAddCurrencyAmount = 999999;

        [Header("Performance")]
        [SerializeField] private int _maxFrameRate = 60;

        public virtual bool EditorForceTouchPlatform => _editorForceTouchPlatform;
        public virtual int DebugAddCurrencyAmount => _debugAddCurrencyAmount;
        public virtual int MaxFrameRate => _maxFrameRate;

        public string ObjectID => nameof(ProjectDataConfigSO);
    }
}
