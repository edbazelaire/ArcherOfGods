using Enums;
using Menu.PopUps.Components.ProfilePopUp;
using Save;
using System;
using System.Collections;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps.PopUps
{
    public class ProfilePopUp : PopUp
    {
        #region Members

        // =============================================================================
        // Data
        int m_CurrentBadgeIndex = -1;

        // =============================================================================
        // GameObjects & Components
        GameObject          m_SelectionWindow;
        GameObject          m_ProfileSection;
        // -- avatar section
        GameObject          m_AvatarSection;
        Button              m_AvatarButton;
        Image               m_AvatarIcon;
        Image               m_AvatarBorder;
        // -- player name & title
        GameObject          m_GamerTagSection;
        TMP_InputField      m_GamerTagInput;
        // -- badges
        GameObject          m_BadgesSection;
        BadgeUI[]           m_Badges;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_SelectionWindow = Finder.Find(gameObject, "SelectionWindow");

            // PROFILE SECTION
            m_ProfileSection = Finder.Find(m_WindowContent, "ProfileSection");
            // -- avatar section
            m_AvatarSection     = Finder.Find(m_ProfileSection, "AvatarSection");
            m_AvatarButton      = Finder.FindComponent<Button>(m_AvatarSection, "AvatarButton");
            m_AvatarIcon        = Finder.FindComponent<Image>(m_AvatarSection, "AvatarIcon");
            m_AvatarBorder      = Finder.FindComponent<Image>(m_AvatarSection, "AvatarBorder");
            // -- gamer tag
            m_GamerTagSection   = Finder.Find(m_ProfileSection, "GamerTagSection");
            m_GamerTagInput     = Finder.FindComponent<TMP_InputField>(m_GamerTagSection, "GamerTagInput");
            // -- badges
            m_BadgesSection     = Finder.Find(m_ProfileSection, "BadgesSection");
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            m_SelectionWindow.SetActive(false);

            // setup Avatar Icon & Border
            m_AvatarIcon.sprite     = AssetLoader.Load<Sprite>("Default", AssetLoader.c_AvatarsPath);
            m_AvatarBorder.sprite   = AssetLoader.Load<Sprite>("Default", AssetLoader.c_BordersPath);

            // setup GamerTag 
            m_GamerTagInput.text = ProfileCloudData.GamerTag;

            // setup Badges
            InitBadges();
        }

        #endregion


        #region GUI Manipulators

        void InitBadges()
        {
            UIHelper.CleanContent(m_BadgesSection);
            m_Badges = new BadgeUI[3];

            for (int i = 0; i < m_Badges.Length; i++)
            {
                m_Badges[i] = Instantiate(AssetLoader.LoadTemplateItem("Badge"), m_BadgesSection.transform).GetComponent<BadgeUI>();
                m_Badges[i].Initialize(ProfileCloudData.GetCurrentBadge(i));

                m_Badges[i].Button.onClick.AddListener(() => OnCurrentBadgeButtonClicked(i));
            }
        }

        /// <summary>
        /// Show/Hide Selection of badge
        /// </summary>
        /// <param name="activate"></param>
        void DisplaySelectionWindow(bool activate)
        {
            m_SelectionWindow.SetActive(activate);
        }

        void SelectBadge(int index)
        {
            DeselectCurrentBadge();

            if (index < 0 || index > m_Badges.Length)
            {
                ErrorHandler.Error("Bades list has no index " + index);
                return;
            }

            m_CurrentBadgeIndex = index;
            m_Badges[m_CurrentBadgeIndex].SetSelected(true);
        }

        void DeselectCurrentBadge()
        {
            if (m_CurrentBadgeIndex < 0)
                return;

            if (m_CurrentBadgeIndex > m_Badges.Length)
            {
                ErrorHandler.Error("Bades list has no index " + m_CurrentBadgeIndex);
                return;
            }

            m_Badges[m_CurrentBadgeIndex].SetSelected(false);
            m_CurrentBadgeIndex = -1;
        }

        #endregion


        #region Public Accessors

        public void Close()
        {
            Exit();
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            m_AvatarButton.onClick.AddListener(OnAvatarButtonClicked);
            m_GamerTagInput.onDeselect.AddListener(OnGamerTagDeselected);
            ProfileCloudData.CurrentBadgeChangedEvent += OnCurrentBadgeChanged;
        }

        protected override void UnRegisterListeners() 
        { 
            base.UnRegisterListeners();

            m_AvatarButton.onClick.RemoveAllListeners();
            ProfileCloudData.CurrentBadgeChangedEvent -= OnCurrentBadgeChanged;

            for (int i = 0; i < m_Badges.Length; i++)
            {
                m_Badges[i].Button.onClick.RemoveAllListeners();
            }
        }

        void OnAvatarButtonClicked()
        {
            Debug.Log("OnAvatarButtonClicked()");
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

                if (! m_SelectionWindow.activeInHierarchy)
                    DisplaySelectionWindow(true);
            }
        }

        void OnCurrentBadgeChanged(int index)
        {
            m_Badges[index].RefreshUI(ProfileCloudData.GetCurrentBadge(index));
        }

        void OnGamerTagDeselected(string newGamerTag)
        {
            if (ProfileCloudData.IsGamerTagValid(newGamerTag, out string reason))
            {
                ProfileCloudData.SetGamerTag(newGamerTag);
                return;
            }

            // TODO : Display why is not valid

            m_GamerTagInput.text = ProfileCloudData.GamerTag;

        }

        #endregion
    }
}