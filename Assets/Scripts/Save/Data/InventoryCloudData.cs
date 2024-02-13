using Enums;
using Inventory;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Save
{
    public class InventoryCloudData : CloudData
    {
        #region Members

        // ===============================================================================================
        // KEYS
        public const string KEY_GOLDS = "Golds";

        // ===============================================================================================
        // EVENTS
        /// <summary> action fired when the amount of gold changed </summary>
        public static Action<int> GoldChangedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { KEY_GOLDS, 0 }
        };

        #endregion


        #region Loading & Saving


        #endregion


        #region Data Manipulators

        public override void SetData(string key, object value)
        {
            base.SetData(key, value);

            if (Enum.TryParse(key, out EReward reward) && reward == EReward.Golds)
            {
                GoldChangedEvent?.Invoke(System.Convert.ToInt32(value));
            }
                
        }

        public void SetData(EReward reward, float value)
        {
            SetData(reward.ToString(), value);
        }


        #endregion

    }
}