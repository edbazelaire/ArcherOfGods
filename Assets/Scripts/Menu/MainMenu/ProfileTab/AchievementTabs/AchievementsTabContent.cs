﻿using Data;
using Game.Loaders;
using Menu.Common.Buttons;
using Menu.MainMenu;
using Tools;
using UnityEngine;

namespace MainMenu.ProfileTab
{
    public class AchievementsTabContent : TabContent
    {
        #region Members

        // GameObjects & Components
        GameObject m_ScrollerContent;
        TemplateAchievementButton m_TemplateAchievementButton;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            m_ScrollerContent = gameObject;
            m_TemplateAchievementButton = AssetLoader.LoadTemplateItem<TemplateAchievementButton>();
        }

        protected override void SetUpUI()
        {
            // reset UI
            UIHelper.CleanContent(m_ScrollerContent);

            foreach (AchievementData achievement in AchievementLoader.Achievements)
            {
                // skip completed achievements 
                if (! achievement.Current.HasValue)
                    continue;

                var go = Instantiate(m_TemplateAchievementButton, m_ScrollerContent.transform);
                go.Initialize(achievement);
            }
        }

        #endregion


        #region GUI Manipulators

        public override void Activate(bool activate)
        {
            base.Activate(activate);

            gameObject.SetActive(activate);
        }

        #endregion
    }
}