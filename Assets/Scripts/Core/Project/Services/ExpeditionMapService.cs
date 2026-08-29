using System.Collections.Generic;
using SpaceInvaders.Scenes.Expedition;
using SpaceInvaders.Scenes.Game;
using Zenject;
using Random = System.Random;

namespace SpaceInvaders.Project
{
    public interface IExpeditionMapService
    {
        /// <summary>Builds a whole map from a seed. The same seed always produces the same map.</summary>
        List<ExpeditionNodeEntry> GenerateMap(int seed);
    }

    /// <summary>
    /// Lays the map out row by row: one start node, one mega boss, and a variable band between them.
    /// Every node is reachable from the start and every node reaches the next row, so no path is a
    /// dead end.
    /// </summary>
    public class ExpeditionMapService : IExpeditionMapService
    {
        [Inject] private readonly IExpeditionRepository _expeditionRepository;

        public List<ExpeditionNodeEntry> GenerateMap(int seed)
        {
            ExpeditionDataConfigSO config = _expeditionRepository.GetExpeditionDataConfig();
            var random = new Random(seed);
            var rows = BuildRows(config, random);

            LinkRows(config, rows, random);
            AssignLevels(config, rows, random);

            var nodes = new List<ExpeditionNodeEntry>();
            foreach (List<ExpeditionNodeEntry> row in rows)
            {
                nodes.AddRange(row);
            }

            return nodes;
        }

        /// <summary>The start and mega boss rows hold one node; the rest are a random width.</summary>
        private static List<List<ExpeditionNodeEntry>> BuildRows(ExpeditionDataConfigSO config, Random random)
        {
            var rows = new List<List<ExpeditionNodeEntry>>();
            int depth = System.Math.Max(config.Depth, 2);
            int nextId = 0;

            for (int rowDepth = 0; rowDepth < depth; rowDepth++)
            {
                bool isSingle = rowDepth == 0 || rowDepth == depth - 1;
                int width = isSingle
                    ? 1
                    : random.Next(config.MinBranchWidth, config.MaxBranchWidth + 1);

                // A special type is used at most once per depth, so a row is never all shops or all events.
                var usedTypes = new HashSet<ExpeditionNodeTypes>();
                var row = new List<ExpeditionNodeEntry>();

                for (int column = 0; column < width; column++)
                {
                    ExpeditionNodeTypes nodeType = GetNodeType(config, rowDepth, depth, usedTypes, random);
                    usedTypes.Add(nodeType);

                    row.Add(new ExpeditionNodeEntry
                    {
                        Id = nextId++,
                        Depth = rowDepth,
                        Column = column,
                        NodeType = nodeType.ToString(),
                        State = ExpeditionNodeStateTypes.Locked.ToString()
                    });
                }

                rows.Add(row);
            }

            rows[0][0].State = ExpeditionNodeStateTypes.Visited.ToString();
            return rows;
        }

        private static ExpeditionNodeTypes GetNodeType(ExpeditionDataConfigSO config, int rowDepth, int depth,
            HashSet<ExpeditionNodeTypes> usedTypes, Random random)
        {
            if (rowDepth == 0)
            {
                return ExpeditionNodeTypes.Start;
            }

            if (rowDepth == depth - 1)
            {
                return ExpeditionNodeTypes.MegaBoss;
            }

            // An authored boss depth still obeys the earliest-boss rule, so one bad number cannot
            // put a boss on the opening rows.
            if (IsAuthoredBossDepth(config, rowDepth) && rowDepth >= config.MinBossDepth)
            {
                return ExpeditionNodeTypes.Boss;
            }

            return RollWeightedNodeType(config, rowDepth, usedTypes, random);
        }

        private static bool IsAuthoredBossDepth(ExpeditionDataConfigSO config, int rowDepth)
        {
            foreach (int bossDepth in config.BossDepths)
            {
                if (bossDepth == rowDepth)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Rolls among the types allowed here, so a weight can never place one where the rules
        /// forbid it. Normal is the fallback and is always allowed.</summary>
        private static ExpeditionNodeTypes RollWeightedNodeType(ExpeditionDataConfigSO config, int rowDepth,
            HashSet<ExpeditionNodeTypes> usedTypes, Random random)
        {
            float total = 0f;
            foreach (ExpeditionNodeWeightDTO weight in config.NodeTypeWeights)
            {
                if (IsNodeTypeAllowed(config, weight.NodeType, rowDepth, usedTypes))
                {
                    total += System.Math.Max(weight.Weight, 0f);
                }
            }

            if (total <= 0f)
            {
                return ExpeditionNodeTypes.Normal;
            }

            double roll = random.NextDouble() * total;
            foreach (ExpeditionNodeWeightDTO weight in config.NodeTypeWeights)
            {
                if (weight.Weight <= 0f || !IsNodeTypeAllowed(config, weight.NodeType, rowDepth, usedTypes))
                {
                    continue;
                }

                if (roll < weight.Weight)
                {
                    return weight.NodeType;
                }

                roll -= weight.Weight;
            }

            return ExpeditionNodeTypes.Normal;
        }

        private static bool IsNodeTypeAllowed(ExpeditionDataConfigSO config, ExpeditionNodeTypes nodeType,
            int rowDepth, HashSet<ExpeditionNodeTypes> usedTypes)
        {
            // Only ever placed by position, never rolled.
            if (nodeType == ExpeditionNodeTypes.Start || nodeType == ExpeditionNodeTypes.MegaBoss)
            {
                return false;
            }

            if (nodeType == ExpeditionNodeTypes.Normal)
            {
                return true;
            }

            if (usedTypes.Contains(nodeType))
            {
                return false;
            }

            if (nodeType == ExpeditionNodeTypes.Shop)
            {
                return rowDepth >= config.MinShopDepth;
            }

            if (nodeType == ExpeditionNodeTypes.Boss)
            {
                return rowDepth >= config.MinBossDepth;
            }

            return true;
        }

        /// <summary>
        /// Every node gets at least one forward link and every node on the next row is linked to at
        /// least once, so nothing is orphaned and nothing dead-ends. Links are then kept in column
        /// order so drawn paths never cross.
        /// </summary>
        /// <summary>
        /// Links each row to the next so that no two paths ever cross. Two links cross when the node
        /// lower down the row reaches higher up the next one, so every node owns a contiguous block of
        /// next-row columns and adjacent blocks may touch at one shared node but never overlap.
        /// </summary>
        private static void LinkRows(ExpeditionDataConfigSO config, List<List<ExpeditionNodeEntry>> rows, Random random)
        {
            for (int rowIndex = 0; rowIndex < rows.Count - 1; rowIndex++)
            {
                List<ExpeditionNodeEntry> row = rows[rowIndex];
                List<ExpeditionNodeEntry> nextRow = rows[rowIndex + 1];

                List<int>[] targetColumns = BuildTargetColumns(row.Count, nextRow.Count);
                AddSharedBoundaries(config, targetColumns, random);

                for (int i = 0; i < row.Count; i++)
                {
                    foreach (int column in targetColumns[i])
                    {
                        row[i].NextNodeIds.Add(nextRow[column].Id);
                    }

                    row[i].NextNodeIds.Sort();
                }
            }
        }

        /// <summary>
        /// Every node starts on its nearest column, then the columns nobody landed on are handed to
        /// whichever neighbour is closest. Because the nearest columns rise with the row, the blocks
        /// come out in order and cover the whole next row, so nothing is orphaned and nothing crosses.
        /// </summary>
        private static List<int>[] BuildTargetColumns(int rowCount, int nextRowCount)
        {
            var targets = new int[rowCount];
            var columns = new List<int>[rowCount];

            for (int i = 0; i < rowCount; i++)
            {
                targets[i] = GetNearestColumn(i, rowCount, nextRowCount);
                columns[i] = new List<int> { targets[i] };
            }

            for (int column = 0; column < nextRowCount; column++)
            {
                int owner = GetOwnerIndex(targets, column);
                if (!columns[owner].Contains(column))
                {
                    columns[owner].Add(column);
                }
            }

            foreach (List<int> nodeColumns in columns)
            {
                nodeColumns.Sort();
            }

            return columns;
        }

        /// <summary>The node whose own column is nearest, so a gap splits between the two around it.</summary>
        private static int GetOwnerIndex(int[] targets, int column)
        {
            int owner = 0;

            for (int i = 1; i < targets.Length; i++)
            {
                if (System.Math.Abs(targets[i] - column) < System.Math.Abs(targets[owner] - column))
                {
                    owner = i;
                }
            }

            return owner;
        }

        /// <summary>
        /// Lets one node of an adjacent pair reach into the other's block by exactly one column, which
        /// is what gives an edge node a real choice. Only ever one direction per boundary: doing both
        /// would make the blocks overlap, and overlapping blocks are what crossing paths look like.
        /// </summary>
        private static void AddSharedBoundaries(ExpeditionDataConfigSO config, List<int>[] targetColumns, Random random)
        {
            for (int i = 0; i < targetColumns.Length - 1; i++)
            {
                if (random.NextDouble() >= config.ExtraLinkChance)
                {
                    continue;
                }

                List<int> lower = targetColumns[i];
                List<int> upper = targetColumns[i + 1];

                if (random.Next(2) == 0)
                {
                    int shared = upper[0];
                    if (!lower.Contains(shared))
                    {
                        lower.Add(shared);
                        lower.Sort();
                    }

                    continue;
                }

                int sharedFromLower = lower[lower.Count - 1];
                if (!upper.Contains(sharedFromLower))
                {
                    upper.Add(sharedFromLower);
                    upper.Sort();
                }
            }
        }

        /// <summary>Maps a column onto the next row's width, so links stay roughly vertical.</summary>
        private static int GetNearestColumn(int column, int width, int nextWidth)
        {
            if (width <= 1)
            {
                return nextWidth / 2;
            }

            float ratio = column / (float)(width - 1);
            return (int)System.Math.Round(ratio * (nextWidth - 1));
        }

        /// <summary>Only nodes that are played carry a level, drawn from the pool for their type and depth.</summary>
        private static void AssignLevels(ExpeditionDataConfigSO config, List<List<ExpeditionNodeEntry>> rows, Random random)
        {
            foreach (List<ExpeditionNodeEntry> row in rows)
            {
                foreach (ExpeditionNodeEntry node in row)
                {
                    node.LevelId = GetLevelId(config, node, random);
                }
            }
        }

        private static string GetLevelId(ExpeditionDataConfigSO config, ExpeditionNodeEntry node, Random random)
        {
            var candidates = new List<LevelConfigSO>();

            foreach (ExpeditionLevelPoolDTO pool in config.LevelPools)
            {
                if (pool.NodeType.ToString() != node.NodeType || node.Depth < pool.MinDepth || node.Depth > pool.MaxDepth)
                {
                    continue;
                }

                candidates.AddRange(pool.Levels);
            }

            if (candidates.Count == 0)
            {
                return string.Empty;
            }

            LevelConfigSO level = candidates[random.Next(candidates.Count)];
            return level == null ? string.Empty : level.ObjectID;
        }
    }
}
