using Data;
using Enums;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace Game.Loaders
{
    public static class AchievementLoader
    {
        #region Members

        static List<Achievement> m_Achievements;

        static List<Achievement> Achievements => m_Achievements;

        #endregion


        #region Init & End
        
        public static void Initialize()
        {
            m_Achievements = AssetLoader.LoadAll<Achievement>(AssetLoader.c_AchievementsPath).ToList();
        }

        #endregion


        #region Filters

        /// <summary>
        /// Get only Achievements that have a linke to the provided StatData
        /// </summary>
        /// <param name="achievements"></param>
        /// <param name="statData"></param>
        /// <returns></returns>
        public static List<Achievement> FilterAchievementsByStatData(List<Achievement> achievements, EStatData statData)
        {
            if (statData == EStatData.None)
                return achievements;

            List<Achievement> filteredAchivements = new();

            foreach (Achievement achievement in achievements)
            {
                if (achievement.StatData == statData)
                    filteredAchivements.Add(achievement);
            }

            return filteredAchivements;
        }

        #endregion

        


    }
}