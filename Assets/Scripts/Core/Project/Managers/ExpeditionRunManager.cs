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

    public enum ExpeditionRunResultTypes
    {
        Defeated,
        Completed
    }

    /// <summary>What a run reached, handed to the scene that reports it.</summary>
    public readonly struct ExpeditionRunResultDTO
    {
        public ExpeditionRunResultTypes Result { get; }
        public int DepthReached { get; }

        public ExpeditionRunResultDTO(ExpeditionRunResultTypes result, int depthReached)
        {
            Result = result;
            DepthReached = depthReached;
        }
    }

    /// <summary>Everything that only means something while an expedition is under way, which is why it
    /// is read through one place that is simply absent when none is.</summary>
    public interface IExpeditionState
    {
        IReadOnlyList<ExpeditionNodeEntry> Nodes { get; }
        float RemainingHealthRatio { get; }

        /// <summary>A level is being played, so the expedition is not resumable from where it stands.</summary>
        bool IsLevelInProgress { get; }
        bool IsOnFinalLevel { get; }

        /// <summary>A map pointing at a level that no longer exists cannot be walked to the end.</summary>
        bool HasMissingLevels { get; }
    }

    public interface IExpeditionRunManager : IInitializable
    {
        /// <summary>Null when no expedition is under way.</summary>
        IExpeditionState CurrentExpedition { get; }

        void StartNewExpedition();
        void AbandonExpedition();
        void EnterNode(int nodeId);
        bool TryGetCurrentLevelSession(out GameSessionDTO session);
        void CompleteCurrentLevel(GameSessionResultDTO result);
        ExpeditionRunResultDTO FinishExpedition(ExpeditionRunResultTypes result);
    }

    /// <summary>
    /// Owns the live run: its map, where the player stands, and how far they have got. Everything the
    /// player earns is held by the usual progression managers against the Expedition profile, so this
    /// stores none of it.
    /// </summary>
    public partial class ExpeditionRunManager : IExpeditionRunManager, IExpeditionState
    {
        [Inject] private readonly ISaveProfileManager _saveProfileManager;
        [Inject] private readonly IExpeditionMapService _mapService;
        [Inject] private readonly IExpeditionRepository _expeditionRepository;
        [Inject] private readonly ILevelsRepository _levelsRepository;
        [Inject] private readonly ICurrencyManager _currencyManager;
        [Inject] private readonly IList<IGameModeScopedManager> _modeScopedManagers;

        private static readonly List<ExpeditionNodeEntry> EmptyNodes = new();

        private IPersistenceManager _persistenceManager;
        private ExpeditionRunSaveData _data;

        /// <summary>The manager itself reads the expedition, so what it hands out never goes stale.</summary>
        public IExpeditionState CurrentExpedition => _data.RunInProgress == null ? null : this;

        public IReadOnlyList<ExpeditionNodeEntry> Nodes => _data.RunInProgress?.Nodes ?? EmptyNodes;
        public float RemainingHealthRatio => _data.RunInProgress?.RemainingHealthRatio ?? 1f;
        public bool IsLevelInProgress => _data.RunInProgress != null && _data.RunInProgress.IsLevelInProgress;
        public bool IsOnFinalLevel => IsNodeOfType(GetCurrentNode(), ExpeditionNodeTypes.MegaBoss);
        public bool HasMissingLevels => GetHasMissingLevels();

        private int CurrentNodeId => _data.RunInProgress?.CurrentNodeId ?? 0;

        public void Initialize()
        {
            _persistenceManager = _saveProfileManager.GetProfile(GameModeTypes.Expedition);
            _data = _persistenceManager.LoadVersioned<ExpeditionRunSaveData>(
                ExpeditionRunSaveData.SaveKey, ExpeditionRunSaveData.CurrentVersion);
        }

        /// <summary>Replaces whatever ran before, so the seed is the only thing the map depends on.</summary>
        public void StartNewExpedition()
        {
            ClearExpedition();

            int seed = Random.Range(int.MinValue, int.MaxValue);
            List<ExpeditionNodeEntry> nodes = _mapService.GenerateMap(seed);

            _data.RunInProgress = new ExpeditionRunInProgressEntry
            {
                Seed = seed,
                Nodes = nodes,
                CurrentNodeId = nodes.Count == 0 ? 0 : nodes[0].Id
            };

            RefreshNodeStates();
            SaveData();
        }

        public void AbandonExpedition()
        {
            ClearExpedition();
            SaveData();
        }

        /// <summary>Only a node carrying a level moves the expedition into one.</summary>
        public void EnterNode(int nodeId)
        {
            if (!IsNodeReachable(nodeId))
            {
                return;
            }

            ExpeditionNodeEntry node = GetNode(nodeId);
            node.State = ExpeditionNodeStateTypes.Visited.ToString();
            _data.RunInProgress.CurrentNodeId = nodeId;
            _data.RunInProgress.IsLevelInProgress = HasLevel(node);

            RefreshNodeStates();
            SaveData();
        }

        /// <summary>Depth stands in for the level number, so progress reads the same in both modes.</summary>
        public bool TryGetCurrentLevelSession(out GameSessionDTO session)
        {
            ExpeditionNodeEntry node = GetCurrentNode();
            if (!HasLevel(node))
            {
                session = default;
                return false;
            }

            session = new GameSessionDTO(GameModeTypes.Expedition, node.Depth, node.LevelId);
            return true;
        }

        /// <summary>Everything a cleared level yields. Reached only by playing one out, so an
        /// expedition that ends in defeat yields nothing.</summary>
        public void CompleteCurrentLevel(GameSessionResultDTO result)
        {
            if (!IsLevelInProgress)
            {
                return;
            }

            StoreStats(result.Stats);
            StoreScrap(result.Score);

            _data.RunInProgress.IsLevelInProgress = false;
            SaveData();
        }

        /// <summary>Recorded and dropped in one step, so nothing is owed to a screen the player may
        /// never reach.</summary>
        public ExpeditionRunResultDTO FinishExpedition(ExpeditionRunResultTypes result)
        {
            var expeditionResult = new ExpeditionRunResultDTO(result, GetCurrentDepth());

            ClearExpedition();
            SaveData();

            return expeditionResult;
        }

        /// <summary>Health carries between nodes as a share, since the maximum changes with progression.</summary>
        private void StoreStats(ShipStats stats)
        {
            if (stats == null || stats.CurrentMaxHealth <= 0)
            {
                return;
            }

            _data.RunInProgress.RemainingHealthRatio = stats.CurrentHealth / (float)stats.CurrentMaxHealth;
        }

        /// <summary>The run's spendable pool, which is the level's score at the authored rate.</summary>
        private void StoreScrap(int score)
        {
            float scrapPerScore = _expeditionRepository.GetExpeditionDataConfig().ScrapPerScore;
            _currencyManager.AddCurrency(Mathf.RoundToInt(score * scrapPerScore));
        }

        /// <summary>A silent check, since a level removed since the run was saved is not an error.</summary>
        private bool GetHasMissingLevels()
        {
            foreach (ExpeditionNodeEntry node in Nodes)
            {
                if (HasLevel(node) && !_levelsRepository.ContainsLevelConfig(node.LevelId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Shops and events resolve on the map, so only a node carrying a level leaves the scene.</summary>
        private static bool HasLevel(ExpeditionNodeEntry node)
        {
            return node != null && !string.IsNullOrEmpty(node.LevelId);
        }

        private static bool IsNodeOfType(ExpeditionNodeEntry node, ExpeditionNodeTypes nodeType)
        {
            return node != null && node.NodeType == nodeType.ToString();
        }

        private int GetCurrentDepth()
        {
            return GetCurrentNode()?.Depth ?? 0;
        }

        private bool IsNodeReachable(int nodeId)
        {
            ExpeditionNodeEntry current = GetCurrentNode();
            return current != null && current.NextNodeIds.Contains(nodeId);
        }

        private ExpeditionNodeEntry GetCurrentNode()
        {
            return GetNode(CurrentNodeId);
        }

        private ExpeditionNodeEntry GetNode(int nodeId)
        {
            return _data.RunInProgress?.Nodes.Find(node => node.Id == nodeId);
        }

        /// <summary>Only what the current node links to can be picked next; visited nodes stay visited
        /// so the walked path keeps reading as one.</summary>
        private void RefreshNodeStates()
        {
            foreach (ExpeditionNodeEntry node in Nodes)
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

        /// <summary>A run owns its whole profile, so its scrap, perks and gear go with it. Dropped as
        /// one object, so a field added to a run can never be left behind for the next one.</summary>
        private void ClearExpedition()
        {
            _data.RunInProgress = null;

            foreach (IGameModeScopedManager modeScopedManager in _modeScopedManagers)
            {
                modeScopedManager.ClearLoadedData();
            }
        }

        private void SaveData()
        {
            _persistenceManager.Save(ExpeditionRunSaveData.SaveKey, _data);
        }
    }
}
