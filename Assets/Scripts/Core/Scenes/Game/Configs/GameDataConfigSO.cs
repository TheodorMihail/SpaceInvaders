using BaseArchitecture.Core;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    [CreateAssetMenu(fileName = "GameDataConfig", menuName = "SpaceInvaders/Data Config/Game Data Config")]
    public class GameDataConfigSO : ScriptableObject, IRepositoryObject
    {
        [Header("Camera")]
        [Tooltip("Screen edges reserved for UI, as a fraction of the view, so ships never move underneath it.")]
        [SerializeField, Range(0f, 0.4f)] private float _sideMarginRatio = 0.05f;
        [SerializeField, Range(0f, 0.4f)] private float _topMarginRatio = 0.1f;
        [SerializeField, Range(0f, 0.4f)] private float _bottomMarginRatio = 0.03f;

        [Tooltip("Viewport height where the enemy region ends and the player's begins. Below the middle gives enemy formations more depth.")]
        [SerializeField, Range(0.1f, 0.9f)] private float _regionDividerRatio = 0.4f;

        public virtual float SideMarginRatio => _sideMarginRatio;
        public virtual float TopMarginRatio => _topMarginRatio;
        public virtual float BottomMarginRatio => _bottomMarginRatio;
        public virtual float RegionDividerRatio => _regionDividerRatio;

        public string ObjectID => nameof(GameDataConfigSO);
    }
}
