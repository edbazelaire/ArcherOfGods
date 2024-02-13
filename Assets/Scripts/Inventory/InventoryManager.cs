using Assets;
using Enums;
using Game.Managers;
using NUnit.Framework.Interfaces;
using Save;
using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Inventory
{
    public static class InventoryManager 
    {
        #region Members

        public const int MAX_CHESTS = 4;

        public static Action<ChestData, int> ChestsAddedEvent;

        static List<SpellItem> m_SpellItems;

        public static int Golds => Convert.ToInt32(Main.CloudSaveManager.InventoryCloudData.Data[InventoryCloudData.KEY_GOLDS]);
        public static ChestData[] Chests => (ChestData[])Main.CloudSaveManager.ChestsCloudData.Data[ChestsCloudData.KEY_CHESTS];

        #endregion


        #region Golds Management

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

            var test = Golds;

            Main.CloudSaveManager.InventoryCloudData.SetData(EReward.Golds, Golds + qty);
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

            Main.CloudSaveManager.InventoryCloudData.SetData(EReward.Golds, Golds - cost);
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

        public static void CollectChest(int index)
        {
            // collect data according to type of chest
            Main.SetPopUp(EPopUpState.ChestOpeningScreen, Chests[index].ChestType);

            // remove chest data from cloud data
            Chests[index] = null;
            Main.CloudSaveManager.ChestsCloudData.SaveValue(ChestsCloudData.KEY_CHESTS);
        } 

        
        #endregion
    }
}