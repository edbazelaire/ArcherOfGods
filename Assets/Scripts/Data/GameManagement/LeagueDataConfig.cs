using Enums;
using Save;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Data.GameManagement
{
    [Serializable]
    public struct SLeagueLevelData
    {
        public int                      NStages;
        public SRewardsData             Rewards;
    }

    [Serializable]
    public struct SLeagueData
    {
        public ELeague                  League;
        public List<SLeagueLevelData>   LevelData;
    }


    [CreateAssetMenu(fileName = "LeagueDataConfig", menuName = "Game/Management/LeagueDataConfig")]
    public class LeagueDataConfig : ScriptableObject
    {
        #region Members

        public static Action<EArenaType, int> LeagueLevelCompletedEvent;

        [SerializeField] List<SLeagueData> m_LeagueData;

        public List<SLeagueData>        LeagueDataList          => m_LeagueData;
        public SLeagueData              CurrentLeagueData       => GetLeagueData(ProgressionCloudData.CurrentLeague);
        public SLeagueLevelData         CurrentLeagueLevelData  => GetLeagueLevelData(ProgressionCloudData.CurrentLeague, ProgressionCloudData.CurrentLeagueLevel);
        public ELeague                  MaxLeague               => ELeague.Champion;

        #endregion


        #region Accessors

        /// <summary>
        /// Get data of the requested level
        /// </summary>
        /// <param name="arenaLevel"></param>
        /// <returns></returns>
        public SLeagueData GetLeagueData(ELeague league)
        {
            foreach (var temp in m_LeagueData)
            {
                if (temp.League == league)
                    return temp;
            }

            ErrorHandler.Error("Unable to find data for league " + league);
            return new SLeagueData();
        }

        /// <summary>
        /// Get data of the requested stage in the requested level
        /// </summary>
        /// <param name="arenaLevel"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public SLeagueLevelData GetLeagueLevelData(ELeague league, int level)
        {
            var leagueData = GetLeagueData(league);
            if (level < 0 || level >= leagueData.LevelData.Count)
            {
                ErrorHandler.Error("Bad league level (" + level + ") for league " + league);
                level = 0;
            }

            return leagueData.LevelData[level];
        }

        public void UpdateLeagueValue(bool up)
        {
            ProgressionCloudData.UpdateLeagueValue(up);
        }

        #endregion
    }
}