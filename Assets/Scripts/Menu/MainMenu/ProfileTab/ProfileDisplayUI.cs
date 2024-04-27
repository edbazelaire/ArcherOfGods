using Enums;
using Menu.Common.Buttons;
using Menu.MainMenu;
using Menu.PopUps.Components.ProfilePopUp;
using Save;
using System.Collections.Generic;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menu.PopUps.PopUps
{
    public class ProfileDisplayUI : MObject
    {
        #region Members

        // =============================================================================
        // Data
        int m_CurrentBadgeIndex = -1;

        // =============================================================================
        // GameObjects & Components
        AchievementRewardsScroller m_AchievementRewardsScroller;

        // -- avatar section
        AvatarButtonUI      m_AvatarButtonUI;
        // -- player name & title
        GameObject          m_GamerTagSection;
        TMP_InputField      m_GamerTagInput;
        // -- title
        Button              m_PlayerTitleButton;
        TMP_Text            m_PlayerTitle;
        // -- badges
        GameObject          m_BadgesSection;
        List<BadgeButtonUI> m_Badges;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_AchievementRewardsScroller = Finder.FindComponent<AchievementRewardsScroller>("AchievementRewardsScroller");

            // -- avatar section
            m_AvatarButtonUI    = Finder.FindComponent<AvatarButtonUI>(gameObject);

            // -- gamer tag
            m_GamerTagSection   = Finder.Find(gameObject, "GamerTagSection");
            m_GamerTagInput     = Finder.FindComponent<TMP_InputField>(m_GamerTagSection, "GamerTagInput");
            // -- title
            m_PlayerTitleButton = Finder.FindComponent<Button>(gameObject, "PlayerTitleButton");
            m_PlayerTitle       = Finder.FindComponent<TMP_Text>(gameObject, "PlayerTitle");
            // -- badges
            m_BadgesSection     = Finder.Find(gameObject, "BadgesSection");
            m_Badges            = Finder.FindComponents<BadgeButtonUI>(m_BadgesSection);
        }

        protected override void SetUpUI()
        {
            // setup Avatar Icon & Border
            m_AvatarButtonUI.Initialize(ProfileCloudData.GetCurrentData(EAchievementReward.Avatar), ProfileCloudData.GetCurrentData(EAchievementReward.Border));

            // setup GamerTag 
            m_GamerTagInput.text = ProfileCloudData.GamerTag;

            // setup Title 
            m_PlayerTitle.text = TextHandler.Split(ProfileCloudData.GetCurrentData(EAchievementReward.Title));
            if (m_PlayerTitle.text == ETitle.None.ToString())
                m_PlayerTitle.text = "";

            // setup Badges
            InitBadges();

            // init reward display scroller
            if (m_AchievementRewardsScroller != null)
                m_AchievementRewardsScroller.Initialize();

            // hide selection window by default
            DisplaySelectionWindow(false);
        }

        #endregion


        #region GUI Manipulators

        void InitBadges()
        {
            for (int i = 0; i < m_Badges.Count; i++)
            {
                m_Badges[i].Initialize(ProfileCloudData.GetCurrentBadge(i));
            }
        }

        /// <summary>
        /// Show/Hide Selection of badge
        /// </summary>
        /// <param name="activate"></param>
        void DisplaySelectionWindow(bool activate)
        {
            if (m_AchievementRewardsScroller == null)
                return;

            m_AchievementRewardsScroller.gameObject.SetActive(activate);
        }

        /// <summary>
        /// Set badge at provided index as currently selected badge that can will be changed when selecting a new badge
        /// </summary>
        /// <param name="index"></param>
        void SelectBadge(int index)
        {
            DeselectCurrentBadge();

            if (index < 0 || index >= m_Badges.Count)
            {
                ErrorHandler.Error("Badges list has no index " + index);
                return;
            }

            ProfileCloudData.LastSelectedBadgeIndex = index;
            m_CurrentBadgeIndex = index;
            m_Badges[m_CurrentBadgeIndex].SetSelected(true);
        }

        void DeselectCurrentBadge()
        {
            if (m_CurrentBadgeIndex < 0)
                return;

            if (m_CurrentBadgeIndex > m_Badges.Count)
            {
                ErrorHandler.Error("Bades list has no index " + m_CurrentBadgeIndex);
                return;
            }

            m_Badges[m_CurrentBadgeIndex].SetSelected(false);
            m_CurrentBadgeIndex = -1;
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            // -- GamerTag
            m_GamerTagInput.onDeselect.AddListener(OnGamerTagDeselected);

            // -- Achievement Reward Profile button
            m_AvatarButtonUI.Button.onClick.AddListener(DisplayRewardCallback(EAchievementReward.Avatar));
            m_PlayerTitleButton.onClick.AddListener(DisplayRewardCallback(EAchievementReward.Title));
            for (int i = 0; i < m_Badges.Count; i++)
            {
                // duplicate i to avoid issue with setting listeners
                int badgeIndex = i;
                m_Badges[i].Button.onClick.AddListener(() => OnCurrentBadgeButtonClicked(badgeIndex));
            }

            // -- External listeners
            ProfileCloudData.CurrentDataChanged += OnCurrentDataChanged;
        }

        protected override void UnRegisterListeners() 
        { 
            base.UnRegisterListeners();

            m_AvatarButtonUI.Button.onClick.RemoveAllListeners();
            m_PlayerTitleButton.onClick.RemoveAllListeners();
            m_GamerTagInput.onDeselect.RemoveAllListeners();

            // -- badges
            for (int i = 0; i < m_Badges.Count; i++)
            {
                m_Badges[i].Button.onClick.RemoveAllListeners();
            }

            ProfileCloudData.CurrentDataChanged -= OnCurrentDataChanged;
        }

        void DisplayRewardType(EAchievementReward achR)
        {
            if (m_AchievementRewardsScroller.gameObject.activeInHierarchy && m_AchievementRewardsScroller.CurrentTab == achR)
                DisplaySelectionWindow(false);
            else
                m_AchievementRewardsScroller.Display(achR);
        }

        UnityAction DisplayRewardCallback(EAchievementReward achR)
        {
            return () => DisplayRewardType(achR);
        }

        void OnCurrentBadgeButtonClicked(int index)
        {
            // Re-click selected : close selection
            if (m_CurrentBadgeIndex == index)
            {
                DeselectCurrentBadge();
                DisplaySelectionWindow(false);
            }

            // Click other than selected 
            if (m_CurrentBadgeIndex != index)
            {
                // set clicked button as selected
                SelectBadge(index);
                DisplayRewardType(EAchievementReward.Badge);
            }
        }

        void OnCurrentDataChanged(EAchievementReward achievementReward)
        {
            string newValue = ProfileCloudData.GetCurrentData(achievementReward);
            
            Debug.Log("OnCurrentDataChanged : " + achievementReward + " - value = " + newValue);

            switch (achievementReward)
            {
                case EAchievementReward.Avatar:
                    m_AvatarButtonUI.SetAvatar(newValue);
                    return;

                case EAchievementReward.Border:
                    m_AvatarButtonUI.SetBorder(newValue);
                    return;

                case EAchievementReward.Title:
                    m_PlayerTitle.text = TextHandler.Split(newValue);
                    return;

                case EAchievementReward.Badge:
                    for (int index = 0; index < m_Badges.Count; index++)
                    {
                        m_Badges[index].RefreshUI(ProfileCloudData.GetCurrentBadge(index));
                    }
                    return;
            }
        }

        void OnGamerTagDeselected(string newGamerTag)
        {
            if (ProfileCloudData.IsGamerTagValid(newGamerTag, out string reason))
            {
                ProfileCloudData.SetGamerTag(newGamerTag);
                return;
            }

            // TODO : Display why is not valid

            // reset value of gamer tag input
            m_GamerTagInput.text = ProfileCloudData.GamerTag;
        }

        #endregion
    }
}