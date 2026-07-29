using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceInvaders.Scenes.Game
{
    /// <summary>Small, domain-agnostic helpers shared across managers (weighted rolls, etc.).</summary>
    public static class GameUtils
    {
        /// <summary>Picks one candidate at random, proportional to each candidate's weight.</summary>
        public static T RollWeighted<T>(IReadOnlyList<T> candidates, Func<T, int> getWeight) where T : class
        {
            int totalWeight = 0;
            foreach (T candidate in candidates)
            {
                totalWeight += getWeight(candidate);
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);

            foreach (T candidate in candidates)
            {
                roll -= getWeight(candidate);
                if (roll < 0)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
