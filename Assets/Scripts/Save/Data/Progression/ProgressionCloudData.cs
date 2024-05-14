using Assets;
using Data.GameManagement;
using Enums;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Services.CloudSave.Models;

namespace Save
{
    [Serializable]
    public struct SArenaCloudData
    {
        public int CurrentLevel;
        public int CurrentStage;

        public SArenaCloudData(int level = 0, int stage = 0)
        {
            CurrentLevel = level;
            CurrentStage = stage;
        }
    }

    public class ProgressionCloudData : CloudData
    {
        #region Members

        public new static ProgressionCloudData Instance => Main.CloudSaveManager.GetCloudData(typeof(ProgressionCloudData)) as ProgressionCloudData;

        // ===============================================================================================
        // CONSTANTS
        public const string KEY_PVP_ELO         = "PvpElo";
        public const string KEY_SOLO_ARENAS     = "SoloArenas";

        // ===============================================================================================
        // ACTIONS
        public static Action<EStatData> StatDataChangedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { KEY_PVP_ELO,              0 },
            { KEY_SOLO_ARENAS,          new Dictionary<EArenaType, SArenaCloudData>() },
        };

        // ===============================================================================================
        // DEPENDENT STATIC ACCESSORS
        public static Dictionary<EArenaType, SArenaCloudData>   SoloArenas     => Instance.m_Data[KEY_SOLO_ARENAS] as Dictionary<EArenaType, SArenaCloudData>;

        #endregion


        #region Loading & Saving

        /// <summary>
        /// Convert SCharacterBuildsCloudData into a dictionnary (easier to manipulate type of data)
        /// </summary>
        /// <param name="charsBuildsList"></param>
        /// <returns></returns>
        protected override object Convert(Item item)
        {
            if (m_Data[item.Key].GetType() == typeof(Dictionary<EArenaType, SArenaCloudData>))
                return item.Value.GetAs<Dictionary<EArenaType, SArenaCloudData>>();

            return base.Convert(item);
        }

        #endregion


        #region Arena Data

        public static void UpdateStageValue(EArenaType arenaType, bool up)
        {
            SArenaCloudData arenaCloudData = SoloArenas[arenaType];

            // Retrieve stage level
            if (! up)
            {
                // stage level is 0 : do nothing
                if (arenaCloudData.CurrentStage == 0)
                    return;

                arenaCloudData.CurrentStage--;
            } 
            
            else
            {
                ArenaData arenaData = AssetLoader.LoadArenaData(arenaType);

                // ADD stage level
                if (arenaCloudData.CurrentStage == arenaData.CurrentArenaLevelData.StageData.Count - 1)
                {
                    UpgradeArenaLevel(arenaType);
                    return;
                }

                arenaCloudData.CurrentStage++;
            }

            SoloArenas[arenaType] = arenaCloudData;
            Instance.SaveValue(KEY_SOLO_ARENAS);
        }

        public static void UpgradeArenaLevel(EArenaType arenaType)
        {
            SArenaCloudData arenaCloudData = SoloArenas[arenaType];
            ArenaData arenaData = AssetLoader.LoadArenaData(arenaType);

            // wait until beeing on MainMenu to fire the arena level event
            int level = arenaCloudData.CurrentLevel;
            Main.AddStoredEvent(EAppState.MainMenu, () => { ArenaData.ArenaLevelCompletedEvent?.Invoke(arenaType, level); });

            if (arenaCloudData.CurrentLevel == arenaData.ArenaLevelData.Count - 1)
                return;

            arenaCloudData.CurrentLevel++;
            arenaCloudData.CurrentStage = 0;

            SoloArenas[arenaType] = arenaCloudData;
            Instance.SaveValue(KEY_SOLO_ARENAS);
        }

        #endregion


        #region Default Data

        void Reset(string key)
        {
            switch (key)
            {
                case KEY_PVP_ELO:
                    m_Data[key] = 0;
                    break;
                
                case KEY_SOLO_ARENAS:
                    var data = new Dictionary<EArenaType, SArenaCloudData>();
                    foreach (EArenaType arenaType in Enum.GetValues(typeof(EArenaType))) 
                    {
                        data[arenaType] = new SArenaCloudData();
                    }

                    m_Data[key] = data;
                    break;
            }
        }

        #endregion



        #region Checkers

        void CheckPvpElo()
        {
            if ((int)m_Data[KEY_PVP_ELO] < 0)
                Reset(KEY_PVP_ELO);
        }

        void CheckArenaData()
        {
            if (SoloArenas.Count == 0)
                Reset(KEY_SOLO_ARENAS);

            foreach (EArenaType arenaType in Enum.GetValues(typeof(EArenaType)))
            {
                if (! SoloArenas.ContainsKey(arenaType))
                {
                    ErrorHandler.Error($"Missing arena in arena {arenaType} data : instantiating new one with default values");
                    SoloArenas[arenaType] = new SArenaCloudData();
                }
            }
        }

        #endregion


        #region Listeners

        protected override void OnCloudDataKeyLoaded(string key)
        {
            base.OnCloudDataKeyLoaded(key);

            if (!m_Data.ContainsKey(key) || m_Data[key] == null)
            {
                ErrorHandler.Error("Missing data " + key + " in cloud data : reseting with default values");
                Reset(key);
                return;
            }

            switch (key)
            {
                case KEY_PVP_ELO:
                    CheckPvpElo();
                    break;

                case KEY_SOLO_ARENAS:
                    CheckArenaData();
                    break;
            }
        }

        #endregion
    }
}