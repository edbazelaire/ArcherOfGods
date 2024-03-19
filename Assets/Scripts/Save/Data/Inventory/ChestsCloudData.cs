using Assets;
using Enums;
using Game.Managers;
using System;
using System.Collections.Generic;
using Unity.Services.CloudSave.Models;

namespace Save
{
    [Serializable]
    public class ChestData
    {
        /// <summary> type of chest </summary>
        public EChestType ChestType;
        /// <summary> timestamp (in seconds) when the chest will be available </summary>
        public long UnlockedAt;

        public ChestData(EChestType chestType, long unlockedAt = 0)
        {
            ChestType = chestType;
            UnlockedAt = unlockedAt;

            if (UnlockedAt <= 0)
                SetUnlockTime();
        }

        public void SetUnlockTime()
        {
            UnlockedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ItemLoader.GetChestRewardData(ChestType).UnlockTime;
        }
    }

    /// <summary>
    /// Accessor of chests data
    /// </summary>
    public class ChestsCloudData : CloudData
    {
        #region Members

        public new static ChestsCloudData Instance => Main.CloudSaveManager.GetCloudData(typeof(ChestsCloudData)) as ChestsCloudData;

        public const string KEY_CHESTS = "Chests";

        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>()
        {
            { KEY_CHESTS, new ChestData[4] { null, null, null, null }  }
        };

        #endregion


        #region Inherited Manipulators

        protected override object Convert(Item item)
        {
            if (m_Data[item.Key].GetType() == typeof(ChestData[]))
                return item.Value.GetAs<ChestData[]>();

            return base.Convert(item);
        }

        #endregion

    }
}