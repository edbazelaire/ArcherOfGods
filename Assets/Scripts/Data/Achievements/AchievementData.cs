using Assets;
using Data.GameManagement;
using Enums;
using Menu.Common.Buttons;
using MyBox;
using Save;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct SAchievementRewardData
    {
        public EAchievementReward AchievementReward;

        [ConditionalField("AchievementReward", false, EAchievementReward.Title)]
        public ETitle   Title;
        [ConditionalField("AchievementReward", false, EAchievementReward.Avatar)]
        public EAvatar  Avatar;
        [ConditionalField("AchievementReward", false, EAchievementReward.Border)]
        public EBorder  Border;
        [ConditionalField("AchievementReward", false, EAchievementReward.Badge)]
        public EBadge   Badge;
        [ConditionalField("AchievementReward", false, EAchievementReward.Badge)]
        public ELeague  League;

        public string Value
        {
            get
            {
                switch (AchievementReward)
                {
                    case EAchievementReward.Title:
                        return Title.ToString();
                    case EAchievementReward.Avatar:
                        return Avatar.ToString();
                    case EAchievementReward.Border:
                        return Border.ToString();

                    case EAchievementReward.Badge:
                        return ProfileCloudData.BadgeToString(Badge, League);

                    default:
                        ErrorHandler.Error("Unahandled case : " + AchievementReward);
                        return "";
                }
            }
        }
    }

    [Serializable]
    public struct SAchievementSubData
    {
        [Description("Treshold value that provides the reward")]
        public float                            MaxValue;

        [Description("Rewards specific to achivements")]
        public List<SAchievementRewardData>     AchivementRewardData;

        [Description("Extra rewards (golds, xp, chests, ...)")]
        public SRewardsData                     Rewards;
    }

    [Serializable]
    public struct SAnalyticsFilter
    {
        public EAnalyticsParam      AnalyticParam;
        public string               Value;
        public EComparator          Comparator;
        
        public SAnalyticsFilter(EAnalyticsParam analyticParam, string value, EComparator comparator = EComparator.Equal)
        {
            AnalyticParam   = analyticParam;
            Value           = value;
            Comparator      = comparator;
        }

        public bool Check(object value, bool validateOnNull = false)
        {
            // CHECK : null value provided
            if (value == null)
                return validateOnNull;

            // CHECK :  equals first
            if (Comparator == EComparator.Equal)
                return Value.Equals(value.ToString());

            // CHECK : is numerical value
            if (! float.TryParse(value.ToString(), out float valueToCheck))
            {
                ErrorHandler.Error($"Using Comparator {Comparator} on a non numerical provided value {value}");
                return false;
            }

            if (! float.TryParse(Value, out float filterValue))
            {
                ErrorHandler.Error($"Using Comparator {Comparator} on a non numerical filter value {Value}");
                return false;
            }

            // switch case comparator
            switch (Comparator)
            {
                case EComparator.Inf:
                    return valueToCheck < filterValue;

                case EComparator.InfEq:
                    return valueToCheck <= filterValue;

                case EComparator.SupEq:
                    return valueToCheck > filterValue;

                case EComparator.Sup:
                    return valueToCheck >= filterValue;

                default:
                    ErrorHandler.Error("Unhandled case : " + Comparator);
                    return false;
            }
        }
    }

    [CreateAssetMenu(fileName = "Achievement", menuName = "Game/Achievements/Achievement")]
    public class AchievementData : ScriptableObject
    {
        #region Members

        [Description("Description informations of the Achievement")]
        public string Description;

        [Description("Stat to record")]
        public EAnalytics Analytics;

        [Description("Stat to record")]
        public List<SAnalyticsFilter> AnalyticsFilters;

        [Description("List of each sub-achiemevents linked to their rewards")]
        public List<SAchievementSubData> AchievementSubData;

        // ===========================================================================================
        // Dependent values
        public string Name => name;
        public float Count              => StatCloudData.GetAnalyticsCount(Analytics, AnalyticsFilters);
        public float RequestedValue     => Current == null ? 0 : Current.Value.MaxValue;
        public bool IsUnlockable        => Current != null && Count >= RequestedValue;

        public SAchievementSubData? Current
        {
            get
            {
                int index = ProfileCloudData.GetAchievementIndex(Name);
                if (index >= AchievementSubData.Count)
                    return null;

                return AchievementSubData[index];
            }
        }


        #endregion


        #region Unlocking

        public void Unlock()
        {
            SAchievementSubData achievementData = Current.Value;

            // ACHIEVEMENT REWARDS
            bool saveAR = false;
            foreach (SAchievementRewardData data in achievementData.AchivementRewardData)
            {
                ErrorHandler.Log("Unlocking AchievementReward : " + data.AchievementReward + " - " + data.Value);
                ProfileCloudData.AddAchievementReward(data.AchievementReward, data.Value, false);
            }
            if (saveAR)
                ProfileCloudData.Instance.SaveValue(ProfileCloudData.KEY_ACHIEVEMENT_REWARDS);

            // save that the achievement was completed
            ProfileCloudData.CompleteAchievement(Name);

            // display rewards
            if (achievementData.AchivementRewardData.Count > 0)
                Main.DisplayAchievementRewards(achievementData.AchivementRewardData);

            // REWARDS
            if (! achievementData.Rewards.IsEmpty)
                Main.DisplayRewards(achievementData.Rewards, ERewardContext.Achievements);
        }

        #endregion

    }
}