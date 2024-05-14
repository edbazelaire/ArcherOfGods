using Enums;
using Save;
using Tools;
using UnityEngine;
using UnityEngine.Events;

namespace Menu.MainMenu
{
    public class ProfileDisplayBE : MObject
    {
        #region Members

        // =============================================================================
        // Data
        int m_CurrentBadgeIndex = -1;

        // =============================================================================
        // GameObjects & Components
        AchievementRewardsScroller      m_AchievementRewardsScroller;
        ProfileDisplayUI                m_ProfileDisplayUI;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_AchievementRewardsScroller    = Finder.FindComponent<AchievementRewardsScroller>("AchievementRewardsScroller");
            m_ProfileDisplayUI              = Finder.FindComponent<ProfileDisplayUI>(gameObject);
        }

        protected override void SetUpUI()
        {
            // initialize UI of the profile
            m_ProfileDisplayUI.Initialize(ProfileCloudData.CurrentProfileData);

            // set all profile disaply ui buttons active
            m_ProfileDisplayUI.SetButtonsActive(true);

            // init reward display scroller
            if (m_AchievementRewardsScroller != null)
                m_AchievementRewardsScroller.Initialize();

            // hide selection window by default
            DisplaySelectionWindow(false);
        }

        #endregion


        #region GUI Manipulators

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

            if (index < 0 || index >= m_ProfileDisplayUI.BadgeButtons.Count)
            {
                ErrorHandler.Error("Badges list has no index " + index);
                return;
            }

            ProfileCloudData.LastSelectedBadgeIndex = index;
            m_CurrentBadgeIndex = index;
            m_ProfileDisplayUI.BadgeButtons[m_CurrentBadgeIndex].SetSelected(true);
        }

        void DeselectCurrentBadge()
        {
            if (m_CurrentBadgeIndex < 0)
                return;

            if (m_CurrentBadgeIndex > m_ProfileDisplayUI.BadgeButtons.Count)
            {
                ErrorHandler.Error("Bades list has no index " + m_CurrentBadgeIndex);
                return;
            }

            m_ProfileDisplayUI.BadgeButtons[m_CurrentBadgeIndex].SetSelected(false);
            m_CurrentBadgeIndex = -1;
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            // -- GamerTag
            m_ProfileDisplayUI.GamerTagInput.onDeselect.AddListener(OnGamerTagDeselected);

            // -- Achievement Reward Profile button
            m_ProfileDisplayUI.AvatarButtonUI.Button.onClick.AddListener(DisplayRewardCallback(EAchievementReward.Avatar));
            m_ProfileDisplayUI.PlayerTitleButton.onClick.AddListener(DisplayRewardCallback(EAchievementReward.Title));

            for (int i = 0; i < m_ProfileDisplayUI.BadgeButtons.Count; i++)
            {
                // duplicate i to avoid issue with setting listeners
                int badgeIndex = i;
                m_ProfileDisplayUI.BadgeButtons[i].Button.onClick.AddListener(() => OnCurrentBadgeButtonClicked(badgeIndex));
            }

            // -- External listeners
            ProfileCloudData.CurrentDataChanged += OnCurrentDataChanged;
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            m_ProfileDisplayUI.AvatarButtonUI.Button.onClick.RemoveAllListeners();
            m_ProfileDisplayUI.PlayerTitleButton.onClick.RemoveAllListeners();
            m_ProfileDisplayUI.GamerTagInput.onDeselect.RemoveAllListeners();

            // -- badges
            for (int i = 0; i < m_ProfileDisplayUI.BadgeButtons.Count; i++)
            {
                m_ProfileDisplayUI.BadgeButtons[i].Button.onClick.RemoveAllListeners();
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
                    m_ProfileDisplayUI.AvatarButtonUI.SetAvatar(newValue);
                    return;

                case EAchievementReward.Border:
                    m_ProfileDisplayUI.AvatarButtonUI.SetBorder(newValue);
                    return;

                case EAchievementReward.Title:
                    m_ProfileDisplayUI.SetTitle(newValue);
                    return;

                case EAchievementReward.Badge:
                    m_ProfileDisplayUI.RefreshBadges(ProfileCloudData.CurrentBadges);
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
            m_ProfileDisplayUI.SetGamerTag(ProfileCloudData.GamerTag);
        }

        #endregion
    }
}