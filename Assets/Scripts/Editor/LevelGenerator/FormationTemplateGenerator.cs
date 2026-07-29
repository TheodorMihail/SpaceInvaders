using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace SpaceInvaders.Editor
{
    public enum FormationTemplateTypes
    {
        Grid,
        VShape,
        Line,
        Diamond,
        Circle,
        Cluster
    }

    public static class FormationTemplateGenerator
    {
        public static List<Vector2Int> Generate(FormationTemplateTypes type, int enemyCount, int spacingX, int spacingY, Random random)
        {
            if (enemyCount <= 0)
            {
                return new List<Vector2Int>();
            }

            switch (type)
            {
                case FormationTemplateTypes.Grid:
                    return GenerateGrid(enemyCount, spacingX, spacingY);
                case FormationTemplateTypes.VShape:
                    return GenerateVShape(enemyCount, spacingX, spacingY);
                case FormationTemplateTypes.Line:
                    return GenerateSymmetricRow(enemyCount, spacingX, 0);
                case FormationTemplateTypes.Diamond:
                    return GenerateDiamond(enemyCount, spacingX, spacingY);
                case FormationTemplateTypes.Circle:
                    return GenerateCircle(enemyCount, spacingX, spacingY, random);
                case FormationTemplateTypes.Cluster:
                    return GenerateCluster(enemyCount, spacingX, spacingY, random);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unhandled formation template type.");
            }
        }

        private static List<Vector2Int> GenerateSymmetricRow(int count, int spacingX, int y)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            if (count <= 0)
            {
                return positions;
            }

            float centerOffset = (count - 1) / 2f;
            for (int i = 0; i < count; i++)
            {
                int x = Mathf.RoundToInt((i - centerOffset) * spacingX);
                positions.Add(new Vector2Int(x, y));
            }

            return positions;
        }

        private static List<Vector2Int> GenerateGrid(int count, int spacingX, int spacingY)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            int columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(count)));
            int remaining = count;
            int row = 0;

            while (remaining > 0)
            {
                int itemsInRow = Mathf.Min(columns, remaining);
                positions.AddRange(GenerateSymmetricRow(itemsInRow, spacingX, row * spacingY));
                remaining -= itemsInRow;
                row++;
            }

            return positions;
        }

        private static List<Vector2Int> GenerateVShape(int count, int spacingX, int spacingY)
        {
            List<Vector2Int> positions = new List<Vector2Int> { Vector2Int.zero };
            int step = 1;
            bool addToLeftArm = true;

            while (positions.Count < count)
            {
                int x = addToLeftArm ? -step * spacingX : step * spacingX;
                int y = step * spacingY;
                positions.Add(new Vector2Int(x, y));

                if (!addToLeftArm)
                {
                    step++;
                }
                addToLeftArm = !addToLeftArm;
            }

            return positions;
        }

        private static List<Vector2Int> GenerateDiamond(int count, int spacingX, int spacingY)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            List<int> rowWidths = BuildDiamondRowWidths(count);
            int remaining = count;

            for (int row = 0; row < rowWidths.Count && remaining > 0; row++)
            {
                int width = Mathf.Min(rowWidths[row], remaining);
                positions.AddRange(GenerateSymmetricRow(width, spacingX, row * spacingY));
                remaining -= width;
            }

            return positions;
        }

        private static List<int> BuildDiamondRowWidths(int count)
        {
            List<int> widths = new List<int>();
            int width = 1;
            int total = 0;

            while (total < count)
            {
                widths.Add(width);
                total += width;
                width += 2;
            }

            width -= 4;
            while (width >= 1)
            {
                widths.Add(width);
                width -= 2;
            }

            return widths;
        }

        private static List<Vector2Int> GenerateCircle(int count, int spacingX, int spacingY, Random random)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            if (count == 1)
            {
                positions.Add(Vector2Int.zero);
                return positions;
            }

            float radius = Mathf.Max(spacingX, spacingY) * count / (2f * Mathf.PI) * 1.2f;
            float startAngle = (float)(random.NextDouble() * Mathf.PI * 2f);

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (Mathf.PI * 2f * i / count);
                int x = Mathf.RoundToInt(Mathf.Cos(angle) * radius);
                int y = Mathf.RoundToInt(Mathf.Sin(angle) * radius);
                positions.Add(new Vector2Int(x, y));
            }

            return positions;
        }

        private static List<Vector2Int> GenerateCluster(int count, int spacingX, int spacingY, Random random)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            int clusterCount = Mathf.Clamp(random.Next(2, 4), 1, count);
            int[] clusterSizes = SplitEvenly(count, clusterCount, random);

            int clusterSpacingX = spacingX * 4;
            float centerOffset = (clusterCount - 1) / 2f;

            for (int clusterIndex = 0; clusterIndex < clusterCount; clusterIndex++)
            {
                int clusterCenterX = Mathf.RoundToInt((clusterIndex - centerOffset) * clusterSpacingX);
                List<Vector2Int> miniGrid = GenerateGrid(clusterSizes[clusterIndex], spacingX, spacingY);

                foreach (Vector2Int slot in miniGrid)
                {
                    positions.Add(new Vector2Int(slot.x + clusterCenterX, slot.y));
                }
            }

            return positions;
        }

        private static int[] SplitEvenly(int total, int parts, Random random)
        {
            int[] sizes = new int[parts];
            int basePart = total / parts;
            int remainder = total % parts;

            for (int i = 0; i < parts; i++)
            {
                sizes[i] = basePart;
            }

            for (int i = 0; i < remainder; i++)
            {
                sizes[random.Next(parts)]++;
            }

            return sizes;
        }
    }
}
