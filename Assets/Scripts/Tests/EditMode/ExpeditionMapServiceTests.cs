using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using SpaceInvaders.Project;
using SpaceInvaders.Scenes.Expedition;
using UnityEngine;
using Zenject;

namespace SpaceInvaders.Tests
{
    [TestFixture]
    public class ExpeditionMapServiceTests : ZenjectUnitTestFixture
    {
        private const int Depth = 10;
        private const int MinBranchWidth = 2;
        private const int MaxBranchWidth = 3;
        private const int MinShopDepth = 2;
        private const int MinBossDepth = 3;

        /// <summary>Authored above the minimum, so it stands.</summary>
        private const int AuthoredBossDepth = 5;

        /// <summary>Authored below the minimum, so it must be ignored.</summary>
        private const int ClampedBossDepth = 1;

        /// <summary>Enough seeds that a rule broken only on some layouts still shows up.</summary>
        private const int SeedSampleCount = 200;

        private IExpeditionMapService _mapService;
        private ExpeditionDataConfigSO _mockConfig;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _mockConfig = CreateConfig(extraLinkChance: 0.5f);

            var mockRepository = Substitute.For<IExpeditionRepository>();
            mockRepository.GetExpeditionDataConfig().Returns(_ => _mockConfig);

            Container.Bind<IExpeditionRepository>().FromInstance(mockRepository);
            _mapService = Container.Instantiate<ExpeditionMapService>();
        }

        [TearDown]
        public override void Teardown()
        {
            Object.DestroyImmediate(_mockConfig);
            base.Teardown();
        }

        [Test]
        public void GenerateMap_WithTheSameSeed_ProducesTheSameMap()
        {
            List<ExpeditionNodeEntry> first = _mapService.GenerateMap(1234);
            List<ExpeditionNodeEntry> second = _mapService.GenerateMap(1234);

            Assert.AreEqual(first.Count, second.Count);

            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].Id, second[i].Id);
                Assert.AreEqual(first[i].Depth, second[i].Depth);
                Assert.AreEqual(first[i].Column, second[i].Column);
                Assert.AreEqual(first[i].NodeType, second[i].NodeType);
                CollectionAssert.AreEqual(first[i].NextNodeIds, second[i].NextNodeIds);
            }
        }

        [Test]
        public void GenerateMap_WithDifferentSeeds_ProducesDifferentMaps()
        {
            List<ExpeditionNodeEntry> first = _mapService.GenerateMap(1);
            List<ExpeditionNodeEntry> second = _mapService.GenerateMap(2);

            Assert.IsFalse(DescribeMap(first) == DescribeMap(second));
        }

        [Test]
        public void GenerateMap_OpensOnASingleStartAndEndsOnASingleMegaBoss()
        {
            ForEachSampledMap(nodes =>
            {
                List<ExpeditionNodeEntry> firstRow = GetRow(nodes, 0);
                List<ExpeditionNodeEntry> lastRow = GetRow(nodes, Depth - 1);

                Assert.AreEqual(1, firstRow.Count);
                Assert.AreEqual(ExpeditionNodeTypes.Start.ToString(), firstRow[0].NodeType);

                Assert.AreEqual(1, lastRow.Count);
                Assert.AreEqual(ExpeditionNodeTypes.MegaBoss.ToString(), lastRow[0].NodeType);
            });
        }

        [Test]
        public void GenerateMap_KeepsEveryBranchWidthWithinTheAuthoredRange()
        {
            ForEachSampledMap(nodes =>
            {
                for (int depth = 1; depth < Depth - 1; depth++)
                {
                    int width = GetRow(nodes, depth).Count;
                    Assert.GreaterOrEqual(width, MinBranchWidth);
                    Assert.LessOrEqual(width, MaxBranchWidth);
                }
            });
        }

        [Test]
        public void GenerateMap_ReachesEveryNodeFromTheStart()
        {
            ForEachSampledMap(nodes =>
            {
                var reached = new HashSet<int> { nodes[0].Id };
                var pending = new Queue<int>();
                pending.Enqueue(nodes[0].Id);

                while (pending.Count > 0)
                {
                    ExpeditionNodeEntry node = FindNode(nodes, pending.Dequeue());
                    foreach (int nextId in node.NextNodeIds)
                    {
                        if (reached.Add(nextId))
                        {
                            pending.Enqueue(nextId);
                        }
                    }
                }

                Assert.AreEqual(nodes.Count, reached.Count, "A node is unreachable from the start.");
            });
        }

        [Test]
        public void GenerateMap_LeavesNoDeadEndBeforeTheMegaBoss()
        {
            ForEachSampledMap(nodes =>
            {
                foreach (ExpeditionNodeEntry node in nodes)
                {
                    if (node.Depth == Depth - 1)
                    {
                        continue;
                    }

                    Assert.IsNotEmpty(node.NextNodeIds, $"Node at depth {node.Depth} leads nowhere.");
                }
            });
        }

        /// <summary>Two links cross when the lower node of a row reaches higher than the upper one, so
        /// adjacent nodes may share a target but never straddle each other.</summary>
        [Test]
        public void GenerateMap_NeverCrossesTwoPaths()
        {
            ForEachSampledMap(nodes =>
            {
                for (int depth = 0; depth < Depth - 1; depth++)
                {
                    List<ExpeditionNodeEntry> row = GetRow(nodes, depth);

                    for (int i = 0; i < row.Count - 1; i++)
                    {
                        int highestOfLower = GetHighestTargetColumn(nodes, row[i]);
                        int lowestOfUpper = GetLowestTargetColumn(nodes, row[i + 1]);

                        Assert.LessOrEqual(highestOfLower, lowestOfUpper,
                            $"Paths cross between columns {row[i].Column} and {row[i + 1].Column} at depth {depth}.");
                    }
                }
            });
        }

        [Test]
        public void GenerateMap_NeverPlacesAShopBeforeItsEarliestDepth()
        {
            AssertNodeTypeNeverAppearsBefore(ExpeditionNodeTypes.Shop, MinShopDepth);
        }

        [Test]
        public void GenerateMap_NeverPlacesABossBeforeItsEarliestDepth()
        {
            AssertNodeTypeNeverAppearsBefore(ExpeditionNodeTypes.Boss, MinBossDepth);
        }

        /// <summary>An authored boss depth is a boss row: every branch leads to one, so the choice is
        /// which path arrives there rather than whether to fight.</summary>
        [Test]
        public void GenerateMap_AtAnAuthoredBossDepth_MakesEveryNodeABoss()
        {
            ForEachSampledMap(nodes =>
            {
                foreach (ExpeditionNodeEntry node in GetRow(nodes, AuthoredBossDepth))
                {
                    Assert.AreEqual(ExpeditionNodeTypes.Boss.ToString(), node.NodeType);
                }
            });
        }

        /// <summary>The guard against a rolled row coming out as one type. Authored boss depths are
        /// uniform by design and are checked separately.</summary>
        [Test]
        public void GenerateMap_NeverRepeatsARolledSpecialTypeWithinOneDepth()
        {
            ForEachSampledMap(nodes =>
            {
                for (int depth = 1; depth < Depth - 1; depth++)
                {
                    if (depth == AuthoredBossDepth)
                    {
                        continue;
                    }

                    var seen = new HashSet<string>();

                    foreach (ExpeditionNodeEntry node in GetRow(nodes, depth))
                    {
                        if (node.NodeType == ExpeditionNodeTypes.Normal.ToString())
                        {
                            continue;
                        }

                        Assert.IsTrue(seen.Add(node.NodeType),
                            $"'{node.NodeType}' appears more than once at depth {depth}.");
                    }
                }
            });
        }

        [Test]
        public void GenerateMap_NeverRollsStartOrMegaBossIntoTheMiddle()
        {
            ForEachSampledMap(nodes =>
            {
                foreach (ExpeditionNodeEntry node in nodes)
                {
                    if (node.Depth == 0 || node.Depth == Depth - 1)
                    {
                        continue;
                    }

                    Assert.AreNotEqual(ExpeditionNodeTypes.Start.ToString(), node.NodeType);
                    Assert.AreNotEqual(ExpeditionNodeTypes.MegaBoss.ToString(), node.NodeType);
                }
            });
        }

        /// <summary>With no chance of a shared boundary every node keeps exactly its own block, which is
        /// the layout most likely to leave an edge node with a single forced path.</summary>
        [Test]
        public void GenerateMap_WithoutSharedBoundaries_StillReachesEveryNode()
        {
            Object.DestroyImmediate(_mockConfig);
            _mockConfig = CreateConfig(extraLinkChance: 0f);

            ForEachSampledMap(nodes =>
            {
                var covered = new HashSet<int>();
                foreach (ExpeditionNodeEntry node in nodes)
                {
                    foreach (int nextId in node.NextNodeIds)
                    {
                        covered.Add(nextId);
                    }
                }

                foreach (ExpeditionNodeEntry node in nodes)
                {
                    if (node.Depth == 0)
                    {
                        continue;
                    }

                    Assert.IsTrue(covered.Contains(node.Id), $"Nothing links to the node at depth {node.Depth}.");
                }
            });
        }

        private void AssertNodeTypeNeverAppearsBefore(ExpeditionNodeTypes nodeType, int minDepth)
        {
            ForEachSampledMap(nodes =>
            {
                foreach (ExpeditionNodeEntry node in nodes)
                {
                    if (node.NodeType != nodeType.ToString())
                    {
                        continue;
                    }

                    Assert.GreaterOrEqual(node.Depth, minDepth, $"'{nodeType}' placed at depth {node.Depth}.");
                }
            });
        }

        private void ForEachSampledMap(System.Action<List<ExpeditionNodeEntry>> assert)
        {
            for (int seed = 0; seed < SeedSampleCount; seed++)
            {
                assert(_mapService.GenerateMap(seed));
            }
        }

        private static ExpeditionDataConfigSO CreateConfig(float extraLinkChance)
        {
            var config = Substitute.For<ExpeditionDataConfigSO>();

            config.Depth.Returns(Depth);
            config.MinBranchWidth.Returns(MinBranchWidth);
            config.MaxBranchWidth.Returns(MaxBranchWidth);
            config.ExtraLinkChance.Returns(extraLinkChance);
            config.MinShopDepth.Returns(MinShopDepth);
            config.MinBossDepth.Returns(MinBossDepth);

            // Deliberately includes a boss depth below the minimum, so the clamp is exercised.
            config.BossDepths.Returns(new List<int> { ClampedBossDepth, AuthoredBossDepth });

            config.NodeTypeWeights.Returns(new List<ExpeditionNodeWeightDTO>());
            config.LevelPools.Returns(new List<ExpeditionLevelPoolDTO>());

            return config;
        }

        private static List<ExpeditionNodeEntry> GetRow(List<ExpeditionNodeEntry> nodes, int depth)
        {
            var row = new List<ExpeditionNodeEntry>();

            foreach (ExpeditionNodeEntry node in nodes)
            {
                if (node.Depth == depth)
                {
                    row.Add(node);
                }
            }

            row.Sort((first, second) => first.Column.CompareTo(second.Column));
            return row;
        }

        private static ExpeditionNodeEntry FindNode(List<ExpeditionNodeEntry> nodes, int nodeId)
        {
            return nodes.Find(node => node.Id == nodeId);
        }

        private static int GetHighestTargetColumn(List<ExpeditionNodeEntry> nodes, ExpeditionNodeEntry node)
        {
            int highest = int.MinValue;

            foreach (int nextId in node.NextNodeIds)
            {
                highest = System.Math.Max(highest, FindNode(nodes, nextId).Column);
            }

            return highest;
        }

        private static int GetLowestTargetColumn(List<ExpeditionNodeEntry> nodes, ExpeditionNodeEntry node)
        {
            int lowest = int.MaxValue;

            foreach (int nextId in node.NextNodeIds)
            {
                lowest = System.Math.Min(lowest, FindNode(nodes, nextId).Column);
            }

            return lowest;
        }

        private static string DescribeMap(List<ExpeditionNodeEntry> nodes)
        {
            var builder = new System.Text.StringBuilder();

            foreach (ExpeditionNodeEntry node in nodes)
            {
                builder.Append(node.Depth).Append(':').Append(node.Column).Append(':').Append(node.NodeType);
                builder.Append(':').Append(string.Join(",", node.NextNodeIds)).Append('|');
            }

            return builder.ToString();
        }
    }
}
