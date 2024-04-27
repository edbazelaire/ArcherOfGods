using Enums;
using Menu.Common.Buttons;
using NUnit.Framework;
using Save;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menu.MainMenu
{
    public class AchievementRewardsScroller : MObject
    {
        #region Members

        Button      m_CancelButton;
        GameObject  m_TabButtonsContainer;
        GameObject  m_ScrollerContent;
        Dictionary<EAchievementReward, AchievementRewardTabButton> m_TabButtons; 

        // Data
        EAchievementReward m_CurrentTab;

        public EAchievementReward CurrentTab => m_CurrentTab;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            m_TabButtonsContainer = Finder.Find(gameObject, "TabButtonsContainer");
            m_ScrollerContent = Finder.Find(gameObject, "ScrollerContent");
            m_CancelButton = Finder.FindComponent<Button>(gameObject, "CancelButton");
        }

        protected override void SetUpUI()
        {
            UIHelper.CleanContent(m_TabButtonsContainer);
            m_TabButtons = new();

            var templateTabButton = AssetLoader.Load<AchievementRewardTabButton>("AchievementRewardTabButton", AssetLoader.c_ProfileTabPath);
            foreach (EAchievementReward ar in Enum.GetValues(typeof(EAchievementReward)))
            {
                if (ar == EAchievementReward.None)
                    continue;

                m_TabButtons[ar] = Instantiate(templateTabButton, m_TabButtonsContainer.transform);
                m_TabButtons[ar].Initialize(ar);
                m_TabButtons[ar].Button.onClick.AddListener(() => Display(ar));
            }
        }

        #endregion


        #region Public Accessors

        public void Display(EAchievementReward achievementReward, bool unlockedOnly = true)
        {
            if (m_CurrentTab == achievementReward)
                return;

            if (! gameObject.activeInHierarchy)
                Activate(true);

            foreach (var item in m_TabButtons)
            {
                item.Value.Activate(item.Key == achievementReward);
            }

            UIHelper.CleanContent(m_ScrollerContent);

            AchievementRewardUI template = AssetLoader.LoadAchievementRewardTemplate(achievementReward);
            foreach (var value in ProfileCloudData.GetAchievementRewards(achievementReward, true))
            {
                var go = Instantiate(template, m_ScrollerContent.transform);
                go.Initialize(value, achievementReward);

                go.Button.onClick.AddListener(() => go.SetAsCurrent());
            }

            if (! unlockedOnly)
            {
                ErrorHandler.Warning("Not unlocked rewards display not done yet");
            }
        }

        public void Activate(bool activate)
        {
            gameObject.SetActive(activate);
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            m_CancelButton.onClick.AddListener(OnCancelButton);
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            m_CancelButton.onClick?.RemoveListener(OnCancelButton);
        }

        void OnCancelButton()
        {
            Activate(false);
        }

        #endregion
    }
}