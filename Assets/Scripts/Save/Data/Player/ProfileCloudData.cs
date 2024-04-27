using Assets;
using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Unity.Services.CloudSave.Models;
using Unity.VisualScripting;

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
        public const    int                 MAX_CHAR_GAMER_TAG      = 20;
        /// <summary> default build if none was created by the player </summary>
        public static   EBadge[]            DEFAULT_BADGES          => new EBadge[N_BADGES_DISPLAYED] { EBadge.None, EBadge.None, EBadge.None }; 
      
        // KEYS ------------------------------------
        public const    string              KEY_GAMER_TAG               = "GamerTag";
        public const    string              KEY_ACHIEVEMENTS            = "Achievements";
        public const    string              KEY_CURRENT_PROFILE_DATA    = "CurrentProfileData";
        public const    string              KEY_CURRENT_BADGES          = "CurrentBadges";
        public const    string              KEY_ACHIEVEMENT_REWARDS     = "AchievementRewards";

        // ===============================================================================================
        // EVENTS
        /// <summary> action fired when the amount of gold changed </summary>
        public static   Action<EAchievementReward, string>  AchievementRewardCollectedEvent;
        public static   Action<string>                      AchievementCompletedEvent;
        public static   Action<EAchievementReward>          CurrentDataChanged;
        public static   Action<int>                         CurrentBadgeChangedEvent;
        public static   Action<EBadge, ELeague>             BadgeUnlockedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { KEY_GAMER_TAG,                ""                                                  },
            { KEY_ACHIEVEMENTS,             new Dictionary<string, int>()                       },
            { KEY_CURRENT_PROFILE_DATA,     new Dictionary<EAchievementReward, string>()        },
            { KEY_CURRENT_BADGES,           new EBadge[3]                                       },
            { KEY_ACHIEVEMENT_REWARDS,      new Dictionary<EAchievementReward, List<string>>()  },
        };

        /// <summary> dictionary of currently unlocked badges linked to max league </summary>
        private Dictionary<EBadge, ELeague> m_Badges;

        // ===============================================================================================
        // DEPENDENT STATIC ACCESSORS
        public static int                                           LastSelectedBadgeIndex = 0;
        public static string                                        GamerTag                => (Instance.m_Data[KEY_GAMER_TAG] as string);
        public static Dictionary<string, int>                       Achievements            => (Instance.m_Data[KEY_ACHIEVEMENTS] as Dictionary<string, int>);
        public static Dictionary<EAchievementReward, string>        CurrentProfileData      => (Instance.m_Data[KEY_CURRENT_PROFILE_DATA] as Dictionary<EAchievementReward, string>);
        public static EBadge[]                                      CurrentBadges           => (Instance.m_Data[KEY_CURRENT_BADGES] as EBadge[]);
        public static Dictionary<EAchievementReward, List<string>>  AchievementRewards      => (Instance.m_Data[KEY_ACHIEVEMENT_REWARDS] as Dictionary<EAchievementReward, List<string>>);
        public static Dictionary<EBadge, ELeague>                   Badges                  => Instance.m_Badges;

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

            if (m_Data[item.Key].GetType() == typeof(Dictionary<string, int>))
                return item.Value.GetAs<Dictionary<string, int>>();

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

        /// <summary>
        /// Get value currently selected as profile
        /// </summary>
        /// <param name="achR"></param>
        /// <returns></returns>
        public static string GetCurrentData(EAchievementReward achR)
        {
            if (CurrentProfileData.ContainsKey(achR))
                return CurrentProfileData[achR];

            if (achR == EAchievementReward.Badge)
                return GetCurrentBadge(LastSelectedBadgeIndex);

            ErrorHandler.Error("Unable to find " + achR + " in current data");
            return "";
        }

        public static void SetCurrentData(EAchievementReward achR, string value)
        {
            CurrentProfileData[achR] = value;
            Instance.SaveValue(KEY_CURRENT_PROFILE_DATA);

            CurrentDataChanged?.Invoke(achR);
        }

        public static void SetCurrentBadge(EBadge badge, int index)
        {
            CurrentBadges[index] = badge;

            // Save & Fire event of the change
            Instance.SaveValue(KEY_CURRENT_BADGES);
            CurrentDataChanged?.Invoke(EAchievementReward.Badge);
        }

        public static string GetCurrentBadge(int index)
        {
            return BadgeToString(CurrentBadges[index], Badges[CurrentBadges[index]]); 
        }

        #endregion


        #region Achievements

        public static int GetAchievementIndex(string achievement)
        {
            if (Achievements.ContainsKey(achievement))
                return Achievements[achievement];

            return 0;
        }

        public static void CompleteAchievement(string achievement)
        {
            if (Achievements.ContainsKey(achievement))
                Achievements[achievement]++;
            else
                Achievements[achievement] = 1;

            Instance.SaveValue(KEY_ACHIEVEMENTS);

            AchievementCompletedEvent?.Invoke(achievement);
        }

        #endregion


        #region Achievement Rewards Data

        /// <summary>
        /// Get all values of a specific achievement reward type
        /// </summary>
        /// <param name="achievementReward"></param>
        /// <returns></returns>
        public static List<string> GetAchievementRewards(EAchievementReward achievementReward, bool highestOnly = false)
        {
            if (! highestOnly || achievementReward != EAchievementReward.Badge)
                return AchievementRewards[achievementReward];

            var values = new List<string>();
            foreach (var item in Badges)
            {
                values.Add(BadgeToString(item.Key, item.Value));
            }

            return values;
        }

        /// <summary>
        /// Add a title, an avatar or a border to the list of unlocked achievements rewards
        /// </summary>
        /// <param name="achievementReward"></param>
        /// <param name="value"></param>
        public static void AddAchievementReward(EAchievementReward achievementReward, string value)
        {
            if (AchievementRewards[achievementReward].Contains(value))
            {
                ErrorHandler.Error("Trying to unlock achievement aready unlocked : " + achievementReward.ToString() + " - " + value);
                return;
            }

            // add & save data
            AchievementRewards[achievementReward].Add(value);
            Instance.SaveValue(KEY_ACHIEVEMENT_REWARDS);

            // fire event that a new reward has been collected
            AchievementRewardCollectedEvent?.Invoke(achievementReward, value);

            // if is badge, update league of badge
            if (achievementReward == EAchievementReward.Badge)
                UpdateBadgeLeague(value);
        }

        void RemoveAchievementReward(EAchievementReward achievementReward, string value)
        {
            if (! AchievementRewards[achievementReward].Contains(value))
            {
                ErrorHandler.Warning("Trying to remove value " + value + " in " + achievementReward.ToString() + " but the value was not found");
                return;
            }

            AchievementRewards[achievementReward].Remove(value);
        } 

        #endregion


        #region Badges

        /// <summary>
        /// Filter values of the same reward to get the highest league unlocked
        /// </summary>
        /// <param name="achievementReward"></param>
        /// <returns></returns>
        Dictionary<EBadge, ELeague> FilterHighestLeague(EAchievementReward achievementReward)
        {
            var filteredDict = new Dictionary<EBadge, ELeague>();
            if (achievementReward != EAchievementReward.Badge)
            {
                ErrorHandler.Warning("FilterHighestLigue() currently not handled for other types than Badge");
                return filteredDict;
            }

            foreach (string valueName in GetAchievementRewards(achievementReward))
            {
                if (!TryGetBadgeFromString(valueName, out EBadge badge, out ELeague league))
                    continue;

                // check that current set value is already sup to provided league
                if (filteredDict.ContainsKey(badge) && filteredDict[badge] > league)
                    continue;

                filteredDict[badge] = league;
            }

            return filteredDict;
        }

        /// <summary>
        /// Add badge to dict of unlocked badge
        /// </summary>
        /// <param name="badge"></param>
        /// <param name="rarety"></param>
        public static void UpdateBadgeLeague(string badgName)
        {
            if (!TryGetBadgeFromString(badgName, out EBadge badge, out ELeague league))
                return;

            // update the value
            Badges[badge] = league;

            // fire event of the change
            BadgeUnlockedEvent?.Invoke(badge, league);
        }

        public static string BadgeToString(EBadge badge, ELeague league)
        {
            return badge.ToString() + (league != ELeague.None ? league.ToString() : "");
        }

        public static (EBadge EBadge, ELeague league) BadgeFromString(string badgeName)
        {
            ELeague league = ELeague.None;
            foreach (ELeague leagueValue in Enum.GetValues(typeof(ELeague)))
            {
                if (badgeName.EndsWith(leagueValue.ToString()))
                {
                    league = leagueValue;
                    badgeName = badgeName[..^league.ToString().Length];
                    break;
                }
            }

            // parse the badge name into the expected enum
            if (!Enum.TryParse(badgeName, out EBadge badge))
            {
                ErrorHandler.Error("Unable to parse badge name (" + badgeName + ") into badge / leage");
                return (EBadge.None, ELeague.None);
            }

            return (badge, league);
        }

        public static bool TryGetBadgeFromString(string badgeName, out EBadge badge, out ELeague league)
        {
            badge = EBadge.None;
            league = ELeague.None;

            if (badgeName == "None")
                return true;

            foreach (ELeague leagueValue in Enum.GetValues(typeof(ELeague)))
            {
                if (leagueValue == ELeague.None)
                    continue;

                if (badgeName.EndsWith(leagueValue.ToString()))
                {
                    league = leagueValue;
                    badgeName = badgeName[..^league.ToString().Length];
                    break;
                }
            }

            // parse the badge name into the expected enum
            if (!Enum.TryParse(badgeName, out badge))
            {
                ErrorHandler.Error("Unable to parse badge name (" + badgeName + ") into badge / leage");
                badge = EBadge.None;
                return false;
            }

            return true;

        }

        #endregion


        #region Helpers

        Type GetTypeOf(EAchievementReward achievementReward)
        {
            switch(achievementReward)
            {
                case EAchievementReward.None:
                    return null;

                case EAchievementReward.Title:
                    return typeof(ETitle);

                case EAchievementReward.Avatar:
                    return typeof(EAvatar);

                case EAchievementReward.Border:
                    return typeof(EBorder);

                case EAchievementReward.Badge:
                    return typeof(EBadge);

                default:
                    ErrorHandler.Error("Unhandled case : " + achievementReward);
                    return null;
            }
        }

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


        #region Set Default Values

        public static void SetDefaultValue(string key) 
        {
            if (key == KEY_CURRENT_PROFILE_DATA)
            {
                Instance.m_Data[key] = new Dictionary<EAchievementReward, string>()
                {
                    { EAchievementReward.Avatar,    EAvatar.None.ToString() },
                    { EAchievementReward.Border,    EBorder.None.ToString() },
                    { EAchievementReward.Title,     ETitle.None.ToString() },
                };
                return;
            }


            if (key == KEY_ACHIEVEMENTS)
            {
                Instance.m_Data[key] = new Dictionary<string, int>() { };
                return;
            }

            if (key == KEY_ACHIEVEMENT_REWARDS)
            {
                Instance.m_Data[key] = new Dictionary<EAchievementReward, List<string>>()
                {
                    { EAchievementReward.Avatar,    new List<string>() { EAvatar.None.ToString() }   },
                    { EAchievementReward.Border,    new List<string>() { EBorder.None.ToString() }   },
                    { EAchievementReward.Title,     new List<string>() { ETitle.None.ToString() }    },
                    { EAchievementReward.Badge,     new List<string>() { EBadge.None.ToString() }    },
                };

                Instance.m_Badges = Instance.FilterHighestLeague(EAchievementReward.Badge);
                return;
            }

            if (key == KEY_CURRENT_BADGES)
            {
                Instance.m_Data[key] = DEFAULT_BADGES;
            }
        }

        #endregion


        #region Checkers

        void CheckGamerTag()
        {
            if (!m_Data.ContainsKey(KEY_GAMER_TAG) || !IsGamerTagValid(GamerTag, out string reason))
                SetGamerTag("Jean Francois Valjean");
        }

        void CheckAchievements()
        {
            if (! m_Data.ContainsKey(KEY_ACHIEVEMENTS) || Achievements == null)
            {
                ErrorHandler.Warning("Current Data are empty : use default ones");
                SetDefaultValue(KEY_ACHIEVEMENTS); 
            }
        }

        void CheckAchievementRewards()
        {
            if (!m_Data.ContainsKey(KEY_ACHIEVEMENT_REWARDS) || AchievementRewards == null || AchievementRewards.Count == 0)
            {
                ErrorHandler.Warning("Achievement Rewards are empty : use default ones");
                SetDefaultValue(KEY_ACHIEVEMENT_REWARDS);
            }

            Dictionary<EAchievementReward, List<string>> valuesToRemove     = new();
            Dictionary<EAchievementReward, List<string>> duplicatesToRemove = new();

            foreach (var item in AchievementRewards)
            {
                // init list of strings
                valuesToRemove[item.Key] = new();
                duplicatesToRemove[item.Key] = new();
                List<string> seenValues = new();

                // get all enums for this type
                var enumNames = Enum.GetNames(GetTypeOf(item.Key));

                // get threw all values in the database and check that they all matches
                foreach (string value in item.Value)
                {
                    bool test = true;

                    if (seenValues.Contains(value)) 
                    {
                        ErrorHandler.Error("Value " + value + " in " + item.Key + " is duplicated");
                        duplicatesToRemove[item.Key].Add(value);
                    }

                    seenValues.Add(value);

                    if (item.Key == EAchievementReward.Badge)
                    {
                        test &= TryGetBadgeFromString(value, out EBadge _, out ELeague _);
                    }

                    else if (!enumNames.Contains(value))
                    {
                        test = false;
                    }

                    if (! test)
                    {
                        ErrorHandler.Error("Unable to find " + value + " in EnumValues of " + item.Key);
                        valuesToRemove[item.Key].Add(value);
                    }
                }
            }

            // REMOVE VALUES
            foreach (var item in valuesToRemove)
                foreach (var value in item.Value)
                    RemoveAchievementReward(item.Key, value);


            // TODO : REMOVE DUPLICATES
        }

        void CheckCurrentData()
        {
            if (!m_Data.ContainsKey(KEY_CURRENT_PROFILE_DATA) || CurrentProfileData == null || CurrentProfileData.Count == 0)
            {
                ErrorHandler.Warning("Current Profile Data are empty : use default ones");
                SetDefaultValue(KEY_CURRENT_PROFILE_DATA);
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

        #endregion


        #region Listeners

        protected override void OnCloudDataKeyLoaded(string key)
        {
            base.OnCloudDataKeyLoaded(key);

            if (key == KEY_CURRENT_PROFILE_DATA)
            {
                CheckCurrentData();
                return;
            }

            if (key == KEY_ACHIEVEMENTS)
            {
                CheckAchievements();
                return;
            }

            if (key == KEY_ACHIEVEMENT_REWARDS)
            {
                CheckAchievementRewards();
                m_Badges = FilterHighestLeague(EAchievementReward.Badge);
                return;
            }

            if (key == KEY_GAMER_TAG)
            {
                CheckGamerTag();
                return;
            }

            if (key == KEY_CURRENT_BADGES)
            {
                CheckCurrentBadges();
                return;
            }
        }

        #endregion
    }
}