using System.Collections.Generic;
using BaseArchitecture.Core;
using SpaceInvaders.Scenes.Game;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Project
{
    public enum ExpeditionNodeTypes
    {
        Start,
        Normal,
        Boss,
        Shop,
        Event,
        MegaBoss
    }

    public enum ExpeditionNodeStateTypes
    {
        /// <summary>Not linked from where the player stands.</summary>
        Locked,
        Available,
        Visited
    }

    /// <summary>Where a run is, which is what decides the state the Expedition scene opens in.</summary>
    public enum ExpeditionRunPhaseTypes
    {
        None,
        OnMap,
        InLevel,
        NodeCleared,
        Finished
    }

    public interface IExpeditionRunManager : IInitializable
    {
        ExpeditionRunPhaseTypes RunPhase { get; }
        IReadOnlyList<ExpeditionNodeEntry> Nodes { get; }
        int CurrentNodeId { get; }
        int CurrentDepth { get; }
        bool HasActiveRun { get; }

        void StartNewRun();
        void AbandonRun();
        bool IsNodeReachable(int nodeId);
        void EnterNode(int nodeId);
    }

    /// <summary>
    /// Owns the live run: its map, where the player stands, and how far they have got. Everything the
    /// player earns is held by the usual progression managers against the Expedition profile, so this
    /// stores none of it.
    /// </summary>
    public partial class ExpeditionRunManager : IExpeditionRunManager
    {
        [Inject] private readonly ISaveProfileManager _saveProfileManager;
        [Inject] private readonly IExpeditionMapService _mapService;

        private IPersistenceManager _persistenceManager;
        private ExpeditionRunSaveData _data;

        public ExpeditionRunPhaseTypes RunPhase => GetRunPhase();
        public IReadOnlyList<ExpeditionNodeEntry> Nodes => _data.Nodes;
        public int CurrentNodeId => _data.CurrentNodeId;
        public int CurrentDepth => GetCurrentDepth();
        public bool HasActiveRun => RunPhase != ExpeditionRunPhaseTypes.None;

        public void Initialize()
        {
            _persistenceManager = _saveProfileManager.GetProfile(GameModeTypes.Expedition);
            _data = _persistenceManager.LoadVersioned<ExpeditionRunSaveData>(
                ExpeditionRunSaveData.SaveKey, ExpeditionRunSaveData.CurrentVersion);
        }

        /// <summary>Replaces whatever ran before, so the seed is the only thing the map depends on.</summary>
        public void StartNewRun()
        {
            ClearRunData();

            _data.Seed = Random.Range(int.MinValue, int.MaxValue);
            _data.Nodes = _mapService.GenerateMap(_data.Seed);
            _data.CurrentNodeId = GetStartNodeId();
            _data.RunPhase = ExpeditionRunPhaseTypes.OnMap.ToString();

            RefreshNodeStates();
            SaveData();
        }

        public void AbandonRun()
        {
            ClearRunData();
            SaveData();
        }

        public bool IsNodeReachable(int nodeId)
        {
            ExpeditionNodeEntry current = GetNode(_data.CurrentNodeId);
            return current != null && current.NextNodeIds.Contains(nodeId);
        }

        public void EnterNode(int nodeId)
        {
            if (!IsNodeReachable(nodeId))
            {
                return;
            }

            ExpeditionNodeEntry node = GetNode(nodeId);
            node.State = ExpeditionNodeStateTypes.Visited.ToString();
            _data.CurrentNodeId = nodeId;

            RefreshNodeStates();
            SaveData();
        }

        private ExpeditionRunPhaseTypes GetRunPhase()
        {
            return System.Enum.TryParse(_data.RunPhase, out ExpeditionRunPhaseTypes phase)
                ? phase
                : ExpeditionRunPhaseTypes.None;
        }

        private int GetCurrentDepth()
        {
            return GetNode(_data.CurrentNodeId)?.Depth ?? 0;
        }

        private ExpeditionNodeEntry GetNode(int nodeId)
        {
            return _data.Nodes.Find(node => node.Id == nodeId);
        }

        private int GetStartNodeId()
        {
            return _data.Nodes.Count == 0 ? 0 : _data.Nodes[0].Id;
        }

        /// <summary>Only what the current node links to can be picked next; visited nodes stay visited
        /// so the walked path keeps reading as one.</summary>
        private void RefreshNodeStates()
        {
            foreach (ExpeditionNodeEntry node in _data.Nodes)
            {
                if (node.State == ExpeditionNodeStateTypes.Visited.ToString())
                {
                    continue;
                }

                ExpeditionNodeStateTypes state = IsNodeReachable(node.Id)
                    ? ExpeditionNodeStateTypes.Available
                    : ExpeditionNodeStateTypes.Locked;

                node.State = state.ToString();
            }
        }

        private void ClearRunData()
        {
            _data.Nodes.Clear();
            _data.Seed = 0;
            _data.CurrentNodeId = 0;
            _data.RemainingHealthRatio = 1f;
            _data.ShopRerollsUsed = 0;
            _data.RunPhase = ExpeditionRunPhaseTypes.None.ToString();
        }

        private void SaveData()
        {
            _persistenceManager.Save(ExpeditionRunSaveData.SaveKey, _data);
        }
    }
}
