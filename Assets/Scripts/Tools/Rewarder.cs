using Assets;
using Data.GameManagement;
using Enums;
using Menu.PopUps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools
{
    public struct SRewardCalculator
    {
        public int Xp;
        public int MinGolds;
        public int MaxGolds;
        public List<SChestDropPercentage> Chests;

        public SRewardCalculator(int xp, int minGolds, int maxGolds, List<SChestDropPercentage> chests)
        {
            Xp          = xp;
            MinGolds    = minGolds;
            MaxGolds    = maxGolds;
            Chests      = chests;
        }

        public int GetXp()
        {
            return Xp;
        }

        public int GetGolds()
        {
            return Random.Range(MinGolds, MaxGolds);
        }

        public List<EChest> GetChests()
        {
            var chests = new List<EChest>();
            foreach (var chestDropPercentage in Chests)
            {
                chests.Add(chestDropPercentage.GetRandomChest());
            }

            return chests;
        }
    }

    public struct SChestDropPercentage
    {
        public Dictionary<EChest, float> ChestsPercentageThresholds;

        public SChestDropPercentage(Dictionary<EChest, float> percentageThresholds)
        {
            ChestsPercentageThresholds = percentageThresholds;
        }

        /// <summary>
        /// Get a random chest from provided percentages
        /// </summary>
        /// <returns></returns>
        public EChest GetRandomChest()
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
            xp: 25,
            minGolds: 30, maxGolds: 55,
            chests: new List<SChestDropPercentage>() {
                new SChestDropPercentage(new Dictionary<EChest, float>
                {
                    { EChest.Common,        0.85f        },
                    { EChest.Rare,          0.99f       },
                    { EChest.Epic,          0.999f      },
                    { EChest.Legendary,     1f          },
                })
            }
        );

        /// <summary> reward when the game is lost </summary>
        public static SRewardCalculator LossGameReward = new SRewardCalculator(
            xp: 5,
            minGolds: 10, maxGolds: 15,
            chests: new List<SChestDropPercentage>() {}
        );

        #endregion


        #region Display Rewards

        /// <summary>
        /// Call the coroutine that displays "ChestOpeningScreen" for each chests in the "SRewards" struct
        /// </summary>
        /// <param name="rewards"></param>
        public static void DisplayRewards(SRewardsData rewards)
        {
            Main.Instance.StartCoroutine(ChestsDisplayCoroutine(rewards.Chests));
        }

        //public static void DisplayListOfSubRewards(List<object> rewards)
        //{
        //    Main.Instance.StartCoroutine(ChestsDisplayCoroutine(rewards.Chests));
        //}

        /// <summary>
        /// Display "ChestOpeningScreen" for each chests in the "SRewards" struct
        /// </summary>
        static IEnumerator ChestsDisplayCoroutine(List<EChest> chests)
        {
            for  (int i = 0; i < chests.Count; i++)
            {
                Main.SetPopUp(EPopUpState.RewardsScreen, chests[i]);

                // wait for ChestOpeningScreen to be displayed
                while (Finder.FindComponent<RewardsScreen>(Main.Canvas.gameObject, throwError: false) == null)
                {
                    yield return null;
                }

                // wait for ChestOpeningScreen to not be diplayed anymore
                while (Finder.FindComponent<RewardsScreen>(Main.Canvas.gameObject, throwError: false) != null)
                {
                    yield return null;
                }
            }
        }

        #endregion
    }
}