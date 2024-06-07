using Assets;
using Data.GameManagement;
using Enums;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Services.CloudSave.Models;

namespace Save
{
    public class NotificationCloudData : CloudData
    {
        #region Members

        public new static NotificationCloudData Instance => Main.CloudSaveManager.GetCloudData(typeof(NotificationCloudData)) as NotificationCloudData;

        // ===============================================================================================
        // CONSTANTS
        public const string KEY_ARENA_REWARDS = "ArenaRewards";

        // ===============================================================================================
        // ACTION
        public static Action ArenaRewardChangedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { KEY_ARENA_REWARDS,        new Dictionary<EArenaType, List<int>>() },
        };

        // ===============================================================================================
        // DEPENDENT STATIC ACCESSORS
        public static Dictionary<EArenaType, List<int>> ArenaRewards => Instance.m_Data[KEY_ARENA_REWARDS] as Dictionary<EArenaType, List<int>>;

        #endregion


        #region Loading & Saving

        /// <summary>
        /// Convert SCharacterBuildsCloudData into a dictionnary (easier to manipulate type of data)
        /// </summary>
        /// <param name="charsBuildsList"></param>
        /// <returns></returns>
        protected override object Convert(Item item)
        {
            if (m_Data[item.Key].GetType() == typeof(Dictionary<EArenaType, List<int>>))
                return item.Value.GetAs<Dictionary<EArenaType, List<int>>>();

            return base.Convert(item);
        }

        #endregion


        #region Arena Data

        public static void AddArenaReward(EArenaType arenaType, int level)
        {
            if (! ArenaRewards.ContainsKey(arenaType))
            {
                ArenaRewards.Add(arenaType, new List<int>() { level });
            } 
            else
            {
                if (ArenaRewards[arenaType].Contains(level))
                {
                    ErrorHandler.Error("Adding rewards for arena " + arenaType + " at level " + level + " but this value was already present in the cloud data");
                    return;
                }

                ArenaRewards[arenaType].Add(level);
            }

            Instance.SaveValue(KEY_ARENA_REWARDS);
            ArenaRewardChangedEvent?.Invoke();
        }

        public static bool CollectArenaReward(EArenaType arenaType, int level)
        {
            if (!ArenaRewards.ContainsKey(arenaType))
            {
                ErrorHandler.Error("Arena type not found in cloud data : " + arenaType);
                return false;
            }

            if (! ArenaRewards[arenaType].Contains(level))
            {
                ErrorHandler.Error("Level "+ level + " not found in arena type cloud data : " + arenaType);
                return false;
            }

            ArenaRewards[arenaType].Remove(level);
            Instance.SaveValue(KEY_ARENA_REWARDS);

            ArenaRewardChangedEvent?.Invoke();
            
            return true;
        }

        public static bool HasRewardsForArenaType(EArenaType arenaType)
        {
            return ArenaRewards.ContainsKey(arenaType) && ArenaRewards[arenaType].Count > 0;
        }

        public static bool HasRewardsForArenaTypeAtLevel(EArenaType arenaType, int level)
        {
            return HasRewardsForArenaType(arenaType) && ArenaRewards[arenaType].Contains(level);
        }

        #endregion


        #region Default Data

        public override void Reset(string key)
        {
            base.Reset(key);

            switch (key)
            {
                case KEY_ARENA_REWARDS:
                    m_Data[key] = new Dictionary<EArenaType, List<int>>();
                    break;
            }
        }

        #endregion


        #region Checkers

        void CheckArenaData()
        {
            
        }

        #endregion


        #region Listeners

        protected override void OnCloudDataKeyLoaded(string key)
        {
            base.OnCloudDataKeyLoaded(key); 
        }

        #endregion
    }
}