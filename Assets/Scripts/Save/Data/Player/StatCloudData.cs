using Assets;
using Enums;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Services.CloudSave.Models;

namespace Save
{
    public class StatCloudData : CloudData
    {
        #region Members

        public new static StatCloudData Instance => Main.CloudSaveManager.GetCloudData(typeof(StatCloudData)) as StatCloudData;

        // ===============================================================================================
        // ACTIONS
        public static Action<EStatData> StatDataChangedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { EStatData.PlayedGames.ToString(), 0 },
            { EStatData.Wins.ToString(),        0 },
        };

        // ===============================================================================================
        // DEPENDENT STATIC ACCESSORS

        #endregion


        #region Loading & Saving

        /// <summary>
        /// Convert SCharacterBuildsCloudData into a dictionnary (easier to manipulate type of data)
        /// </summary>
        /// <param name="charsBuildsList"></param>
        /// <returns></returns>
        protected override object Convert(Item item)
        {
            return base.Convert(item);
        }

        #endregion


        #region Accessors

        public static float GetData(EStatData statData)
        {
            if (! Instance.m_Data.ContainsKey(statData.ToString()))
            {
                ErrorHandler.Error("Unabel to find statData " + statData + " in cloud data");
                return 0f;
            }

            if (float.TryParse(Instance.m_Data[statData.ToString()].ToString(), out float value))
                return value;

            ErrorHandler.Error("Unable to read " + statData.ToString() + " with value " + value + " as float");
            return 0f;
        }

        public static void SetData(EStatData statData, float value)
        {
            Instance.m_Data[statData.ToString()] = value;   
            Instance.SaveValue(statData.ToString());

            StatDataChangedEvent?.Invoke(statData);
        }

        #endregion


        #region Checkers


        #endregion


        #region Listeners

        protected override void OnCloudDataKeyLoaded(string key)
        {
            base.OnCloudDataKeyLoaded(key);
        }

        #endregion
    }
}