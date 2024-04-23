using Assets;
using Enums;
using Menu.PopUps.Components.ProfilePopUp;
using System;
using System.Collections.Generic;
using System.Text;
using Tools;
using Unity.Services.CloudSave.Models;
using Unity.VisualScripting;
using UnityEngine;

namespace Save
{
    public class ProfileCloudData : CloudData
    {
        #region Members

        public new static ProfileCloudData Instance => Main.CloudSaveManager.GetCloudData(typeof(ProfileCloudData)) as ProfileCloudData;

        // ===============================================================================================
        // CONSTANTS
        /// <summary> number of spells in one build </summary>
        public const    int                 N_BADGES_DISPLAYED      = 3;
        public const    int                 MIN_CHAR_GAMER_TAG      = 5;
        public const    int                 MAX_CHAR_GAMER_TAG      = 12;
        /// <summary> default build if none was created by the player </summary>
        public static   EBadge[]            DEFAULT_BADGES          => new EBadge[N_BADGES_DISPLAYED] { EBadge.None, EBadge.None, EBadge.None }; 
      
        // KEYS ------------------------------------
        public const    string              KEY_GAMER_TAG           = "GamerTag";
        public const    string              KEY_CURRENT_DATA        = "CurrentData";
        public const    string              KEY_CURRENT_BADGES      = "CurrentBadges";
        public const    string              KEY_ACHIEVEMENT_REWARDS = "AchievementRewards";
        public const    string              KEY_BADGES              = "Badges";

        // ===============================================================================================
        // EVENTS
        /// <summary> action fired when the amount of gold changed </summary>
        public static   Action<EAchievementReward, string>  AchievementUnlockedEvent;
        public static   Action<int>                         CurrentBadgeChangedEvent;
        public static   Action<EBadge, ELeague>             BadgeUnlockedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { KEY_GAMER_TAG,            ""                                                  },
            { KEY_CURRENT_DATA,         new Dictionary<EAchievementReward, string>()        },
            { KEY_CURRENT_BADGES,       new EBadge[3]                                       },
            { KEY_ACHIEVEMENT_REWARDS,  new Dictionary<EAchievementReward, List<string>>()  },
            { KEY_BADGES,               new Dictionary<EBadge, ELeague>()                   }
        };

        // ===============================================================================================
        // DEPENDENT STATIC ACCESSORS
        /// <summary> get currently selected character </summary>
        public static string                                        GamerTag                => (Instance.m_Data[KEY_GAMER_TAG] as string);
        public static Dictionary<EAchievementReward, string>        CurrentData             => (Instance.m_Data[KEY_CURRENT_DATA] as Dictionary<EAchievementReward, string>);
        public static EBadge[]                                      CurrentBadges           => (Instance.m_Data[KEY_CURRENT_BADGES] as EBadge[]);
        public static Dictionary<EAchievementReward, List<string>>  AchievementRewards      => (Instance.m_Data[KEY_ACHIEVEMENT_REWARDS] as Dictionary<EAchievementReward, List<string>>);
        public static Dictionary<EBadge, ELeague>                   Badges                  => (Instance.m_Data[KEY_BADGES] as Dictionary<EBadge, ELeague>);

        #endregion


        #region Loading & Saving

        /// <summary>
        /// Convert SCharacterBuildsCloudData into a dictionnary (easier to manipulate type of data)
        /// </summary>
        /// <param name="charsBuildsList"></param>
        /// <returns></returns>
        protected override object Convert(Item item)
        {
            if (m_Data[item.Key].GetType() == typeof(Dictionary<EBadge, ELeague>))
                return item.Value.GetAs<Dictionary<EBadge, ELeague>>();

            if (m_Data[item.Key].GetType() == typeof(Dictionary<EAchievementReward, List<string>>))
                return item.Value.GetAs<Dictionary<EAchievementReward, List<string>>>();

            if (m_Data[item.Key].GetType() == typeof(Dictionary<EAchievementReward, string>))
                return item.Value.GetAs<Dictionary<EAchievementReward, string>>();

            return base.Convert(item);
        }

        #endregion


        #region Current Selected Data Manipulator

        /// <summary>
        /// Set Rune of the current build
        /// </summary>
        /// <param name="rune"></param>
        public static void SetGamerTag(string gamerTag)
        {
            Instance.m_Data[KEY_GAMER_TAG] = gamerTag;
            Instance.SaveValue(KEY_GAMER_TAG);
        }

        public static void SetCurrentData(EAchievementReward achR, string value)
        {
            CurrentData[achR] = value;
            Instance.SaveValue(KEY_CURRENT_DATA);
        }

        public static void SetCurrentBadge(EBadge badge, int index)
        {
            CurrentBadges[index] = badge;

            // Save & Fire event of the change
            Instance.SaveValue(KEY_CURRENT_BADGES);
            CurrentBadgeChangedEvent?.Invoke(index);
        }

        public static (EBadge, ELeague) GetCurrentBadge(int index)
        {
            return (CurrentBadges[index], Badges[CurrentBadges[index]]); 
        }

        #endregion


        #region Achievement Data

        public static List<string> GetAchievementRewards(EAchievementReward achievementReward)
        {
            return AchievementRewards[achievementReward];
        }

        public static void UnlockAchievement(EAchievementReward achievementReward, string value)
        {
            if (AchievementRewards[achievementReward].Contains(value))
            {
                ErrorHandler.Error("Trying to unlock achievement aready unlocked : " + achievementReward.ToString() + " - " + value);
                return;
            }

            AchievementRewards[achievementReward].Add(value);
            AchievementUnlockedEvent?.Invoke(achievementReward, value);
        }

        public static void UnlockBadge(EBadge badge, ELeague rarety)
        {
            // update the value
            Badges[badge] = rarety;

            // Save & Fire event of the change
            Instance.SaveValue(KEY_BADGES);
            BadgeUnlockedEvent?.Invoke(badge, rarety);
        }

        #endregion


        #region Public Checkers

        /// <summary>
        /// 
        /// </summary>
        public static bool IsGamerTagValid(string gamerTag, out string reason)
        {
            reason = "";
            if (gamerTag.Length < MIN_CHAR_GAMER_TAG || gamerTag.Length > MAX_CHAR_GAMER_TAG)
            {
                reason = TextLocalizer.LocalizeText("gamer tag must have between " + MIN_CHAR_GAMER_TAG + " and " + MIN_CHAR_GAMER_TAG + " characters");
                return false;
            }

            return true;
        }

        #endregion


        #region Checkers

        void CheckGamerTag()
        {
            if (!m_Data.ContainsKey(KEY_GAMER_TAG) || !IsGamerTagValid(GamerTag, out string reason))
                SetGamerTag("Jean François Valjean");
        }

        void CheckCurrentData()
        {
            if (!m_Data.ContainsKey(KEY_CURRENT_DATA) || CurrentData == null)
            {
                ErrorHandler.Warning("Current Data are empty : use default ones");
                m_Data[KEY_CURRENT_DATA] = new Dictionary<EAchievementReward, string>()
                {
                    { EAchievementReward.Avatar,    "Default" },
                    { EAchievementReward.Border,    "Default" },
                    { EAchievementReward.Title,     "" },
                };
            }
        }

        void CheckCurrentBadges()
        {
            if (! m_Data.ContainsKey(KEY_CURRENT_BADGES) || CurrentBadges == null || CurrentBadges.Length == 0)
            {
                ErrorHandler.Warning("Current Badges are empty : use default ones");
                m_Data[KEY_CURRENT_BADGES] = DEFAULT_BADGES;
            }
        }

        void CheckBadges()
        {
            if (!m_Data.ContainsKey(KEY_BADGES) || Badges == null || Badges.Count == 0)
            {
                ErrorHandler.Warning("List of all Badges is empty : reset");
                m_Data[KEY_BADGES] = new Dictionary<EBadge, ELeague> { { EBadge.None, ELeague.None } };
            }
        }

        #endregion


        #region Listeners

        protected override void OnCloudDataKeyLoaded(string key)
        {
            base.OnCloudDataKeyLoaded(key);

            if (key == KEY_CURRENT_DATA)
            {
                CheckCurrentData();
                return;
            }

            if (key == KEY_CURRENT_BADGES)
            {
                CheckGamerTag();
                return;
            }

            if (key == KEY_CURRENT_BADGES)
            {
                CheckCurrentBadges();
                return;
            }

            if (key == KEY_BADGES)
            {
                CheckBadges();
                return;
            }
        }

        #endregion
    }
}