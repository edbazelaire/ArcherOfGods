using Enums;
using Inventory;
using Save;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct SAchievementRewardData
    {
        public EAchievementReward AchievementReward;
        public string Value;
    }

    [Serializable]
    public struct SAchievementData
    {
        [Description("Was this Achievement already completed ? ")]
        public bool                 IsUnlocked;

        [Description("Treshold value that provides the reward")]
        public float                Value;

        [Description("Reward Badge")]
        public EBadge Badge;

        [Description("League of the badge")]
        public ELeague League;

        [Description("Rewards specific to achivements")]
        public List<SAchievementRewardData> AchivementRewardData;

        [Description("Extra rewards (golds, xp, chests, ...)")]
        public List<SReward>        Rewards;
    }

    [CreateAssetMenu(fileName = "Achievement", menuName = "Game/Achievements/Achievement")]
    public class Achievement : ScriptableObject
    {
        #region Members

        [Description("Description informations of the Achievement")]
        public string Description;

        [Description("Stat to record")]
        public EStatData StatData;

        [Description("List of each sub-achiemevents linked to their rewards")]
        public List<SAchievementData> AchievementData;

        #endregion


        #region Public Acessors

        public SAchievementData? Current
        {
            get
            {
                foreach (var data in AchievementData)
                {
                    if (!data.IsUnlocked)
                        return data;
                }

                return null;
            }
        }

        #endregion


        #region Unlocking

        public bool CheckCompletion()
        {
            if (Current == null)
                return false;

            float value = StatCloudData.GetData(StatData);

            if (value >= Current.Value.Value)
            {
                return true;
            }

            return false;
        }

        public void Unlock()
        {
            SAchievementData achievementData = Current.Value;

            foreach (SAchievementRewardData data in achievementData.AchivementRewardData)
            {
                ProfileCloudData.UnlockAchievement(data.AchievementReward, data.Value);
            }
            
            if (achievementData.Badge != EBadge.None) 
            {
                ProfileCloudData.UnlockBadge(achievementData.Badge, achievementData.League);
            }

            achievementData.IsUnlocked = true;
        }

        #endregion

    }
}