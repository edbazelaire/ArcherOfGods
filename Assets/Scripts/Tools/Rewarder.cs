using Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools
{
    public struct SRewardCalculator
    {
        public int MinGolds;
        public int MaxGolds;
        public List<SChestDropPercentage> Chests;

        public SRewardCalculator(int minGolds, int maxGolds, List<SChestDropPercentage> chests)
        {
            MinGolds    = minGolds;
            MaxGolds    = maxGolds;
            Chests      = chests;
        }

        public int GetGolds()
        {
            return Random.Range(MinGolds, MaxGolds);
        }

        public List<EChestType> GetChests()
        {
            var chests = new List<EChestType>();
            foreach (var chestDropPercentage in Chests)
            {
                chests.Add(chestDropPercentage.GetRandomChest());
            }

            return chests;
        }
    }

    public struct SChestDropPercentage
    {
        public Dictionary<EChestType, float> ChestsPercentageThresholds;

        public SChestDropPercentage(Dictionary<EChestType, float> percentageThresholds)
        {
            ChestsPercentageThresholds = percentageThresholds;
        }

        /// <summary>
        /// Get a random chest from provided percentages
        /// </summary>
        /// <returns></returns>
        public EChestType GetRandomChest()
        {
            float rand = Random.Range(0.0f, 1.0f);
            foreach (var item in ChestsPercentageThresholds)
            {
                if (rand <= item.Value)
                    return item.Key; 
            }

            ErrorHandler.Error("Unable to find any chest matching rand value : " + rand);
            return ChestsPercentageThresholds.Keys.ToList()[ChestsPercentageThresholds.Count - 1];
        }
    }

    public static class Rewarder
    {
        #region Members

        /// <summary> reward when the game is won </summary>
        public static SRewardCalculator WinGameReward = new SRewardCalculator(
            30, 55, 
            new List<SChestDropPercentage>() { 
                new SChestDropPercentage(new Dictionary<EChestType, float>
                {
                    { EChestType.Common,        0.9f        },
                    { EChestType.Rare,          0.99f       },
                    { EChestType.Epic,          0.999f      },
                    { EChestType.Legendary,     1f          },
                }) 
            }
        );

        /// <summary> reward when the game is lost </summary>
        public static SRewardCalculator LossGameReward = new SRewardCalculator(
            10, 15, 
            new List<SChestDropPercentage>() {}
        );

        #endregion
    }
}