using Data;
using Enums;
using Game.Loaders;
using Menu.Common.Buttons;
using Menu.PopUps.PopUps;
using Save;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MainMenu
{
    public class ProfileTab : MainMenuTabContent
    {
        #region Members

        ProfileDisplayUI    m_ProfileDisplay;
        ScrollRect          m_AchievementScroller;
        GameObject          m_AchievementsContainer;

        TemplateAchievementButton m_TemplateAchievementButton;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_ProfileDisplay            = Finder.FindComponent<ProfileDisplayUI>(gameObject, "ProfileDisplay");
            m_AchievementScroller       = Finder.FindComponent<ScrollRect>(gameObject, "AchievementScroller");
            m_AchievementsContainer     = Finder.Find(m_AchievementScroller.gameObject, "AchievementsContainer");

            m_TemplateAchievementButton = AssetLoader.LoadTemplateItem<TemplateAchievementButton>();
        }

        protected override void SetUpUI()
        {
            InitAchievementScroller();
            m_ProfileDisplay.Initialize();
        }

        #endregion


        #region GUI Manipulators

        void InitAchievementScroller()
        {
            // reset UI
            UIHelper.CleanContent(m_AchievementsContainer);

            foreach (AchievementData achievement in AchievementLoader.Achievements)
            {
                var go = Instantiate(m_TemplateAchievementButton, m_AchievementsContainer.transform);
                go.Initialize(achievement);
            }
        }

        #endregion


        #region Listeners
     

        #endregion
    }
}