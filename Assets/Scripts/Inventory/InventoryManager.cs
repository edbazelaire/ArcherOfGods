using Assets;
using Enums;
using Save;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace Inventory
{
    public static class InventoryManager
    {
        #region Members

        // =================================================================================================
        // CONSTANTS
        public const        int                     MAX_CHESTS      = 4;
        public static       ERewardType[]           CURRENCIES      => new ERewardType[] { ERewardType.Golds };

        // =================================================================================================
        // EVENTS
        public static       Action<ChestData, int>  ChestsAddedEvent;

        // =================================================================================================
        // ACCESSORS
        public static       List<SSpellCloudData>   SpellData       => (List<SSpellCloudData>)Main.CloudSaveManager.InventoryCloudData.Data[ERewardType.Spell.ToString()];
        public static       int                     Golds           => Convert.ToInt32(Main.CloudSaveManager.InventoryCloudData.Data[InventoryCloudData.KEY_GOLDS]);
        public static       ChestData[]             Chests          => (ChestData[])Main.CloudSaveManager.ChestsCloudData.Data[ChestsCloudData.KEY_CHESTS];

        #endregion


        #region Currency Management

        public static int GetCurrency(ERewardType rewardType)
        {
            if (! CURRENCIES.Contains(rewardType))
            {
                ErrorHandler.Error("Reward " + rewardType + " is not a currency");
                return 0;
            }

            return (int)Main.CloudSaveManager.InventoryCloudData.Data[rewardType.ToString()];
        }

        public static void UpdateCurrency(ERewardType reward, int amount) 
        {
            var data = Main.CloudSaveManager.InventoryCloudData.Data;

            if (!data.ContainsKey(reward.ToString()))
            {
                ErrorHandler.Error("Currency " + reward + " not found in inventory cloud data");
                return;
            }
            
            var total = (int)data[reward.ToString()] + amount;
            if (amount < 0)
            {
                ErrorHandler.Error($"Not enought {reward} ({(int)data[reward.ToString()]}) to spend ({amount})");
                return;
            }

            Main.CloudSaveManager.InventoryCloudData.SetData(reward, total);
        }

        public static bool CanBuy(int cost)
        {
            return Golds - cost < 0;
        }

        public static void AddGolds(int qty)
        {
            if (qty < 0)
            {
                ErrorHandler.Error($"Trying to add a negative amount of gold ({qty}) : use the Spend() method");
                return;
            }

            Main.CloudSaveManager.InventoryCloudData.SetData(ERewardType.Golds, Golds + qty);
        }

        public static void Spend(int cost)
        {
            if (cost < 0)
            {
                ErrorHandler.Error($"Trying to add a spend a negative amount of gold ({cost}) : use the AddGolds() method");
                return;
            }

            if (! CanBuy(cost)) 
            {
                ErrorHandler.Error($"Not enought golds ({Golds}) to buy the item ({cost}) : this situation should not happen");
                return;
            }

            Main.CloudSaveManager.InventoryCloudData.SetData(ERewardType.Golds, Golds - cost);
        }

        #endregion


        #region Spell Management

        public static SSpellCloudData GetSpellData(ESpell spell)
        {
            return Main.CloudSaveManager.InventoryCloudData.GetSpell(spell);
        }

        #endregion


        #region Chests Management

        public static bool GetFirstAvailableIndex(out int index)
        {
            index = -1; 
            for (int i = 0; i < Chests.Length; i++)
            {
                if (Chests[i] == null)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        public static ChestData CreateRandomChest()
        {
            Array values = Enum.GetValues(typeof(EChestType));
            var random = new System.Random();
            return new ChestData((EChestType)values.GetValue(random.Next(values.Length)));
        }

        public static void AddChest(ChestData chestData)
        {
            if (!GetFirstAvailableIndex(out int index))
                return;

            // add data to list of chest data
            Chests[index] = chestData;

            // fire event that a chest has been added
            ChestsAddedEvent?.Invoke(chestData, index);

            // call for async save of the updated value
            Main.CloudSaveManager.ChestsCloudData.SaveValue(ChestsCloudData.KEY_CHESTS);
        }

        public static void RemoveChestAtIndex(int index)
        {
            // this can happen if the negative index check was not done before the call
            if (index < 0)
            {
                ErrorHandler.Warning("Trying to remove chest with index " + index + ". This should be handled before calling this method");
                return;
            }

            // this should never happen
            if (index >= Chests.Length)
            {
                ErrorHandler.Error("Trying to remove chest with " + index);
                return;
            }

            // remove chest data from cloud data
            Chests[index] = null;
            Main.CloudSaveManager.ChestsCloudData.SaveValue(ChestsCloudData.KEY_CHESTS);
        } 

        /// <summary>
        /// Add a list of rewards to the inventory
        /// </summary>
        /// <param name="rewards"></param>
        public static void CollectRewards(List<SReward> rewards)
        {
            foreach (SReward reward in rewards) 
            {
                CollectReward(reward);
            }
        }

        /// <summary>
        /// Add a reward to the inventory
        /// </summary>
        /// <param name="reward"></param>
        public static void CollectReward(SReward reward)
        {
            if (CURRENCIES.Contains(reward.RewardType))
            {
                UpdateCurrency(reward.RewardType, reward.Qty);
                return;
            }

            if (reward.RewardType == ERewardType.Spell)
            {
                var spelLCloudData = Main.CloudSaveManager.InventoryCloudData.GetSpell((ESpell)reward.Metadata[SReward.METADATA_KEY_SPELL_TYPE]);
                spelLCloudData.Qty += reward.Qty;
                Main.CloudSaveManager.InventoryCloudData.SetSpell(spelLCloudData);
                return;
            }

            ErrorHandler.Error("Unhandled case : " + reward.RewardType);
        }


        #endregion
    }
}