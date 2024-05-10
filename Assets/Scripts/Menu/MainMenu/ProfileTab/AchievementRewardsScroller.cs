using Enums;
using Save;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MainMenu
{
    [Serializable]
    public struct SGridLayoutItem
    {
        public string ScrollItemType;
        public float WidthHeightRatio;
        public int NumColumns;

        public SGridLayoutItem(string scrollItemType, float widthHeightRatio = 0, int numColumns = 0)
        {
            ScrollItemType      = scrollItemType;
            WidthHeightRatio    = widthHeightRatio;
            NumColumns          = numColumns;
        }

        public void UpdateGridLayout(GameObject container)
        {
            GridLayoutGroup layoutGroup = Finder.FindComponent<GridLayoutGroup>(container);
            if (layoutGroup == null)
                return;

            float elemWidth = layoutGroup.cellSize.x;
            if (NumColumns > 0)
            {
                UIHelper.GetSize(layoutGroup.gameObject, out float width, out float _);
                // re-calculate width of the elements
                elemWidth =
                    (
                        width                                                       // base width of the container
                        - layoutGroup.padding.right - layoutGroup.padding.left      // remove padding
                        - (layoutGroup.spacing.x * (NumColumns-1))                  // remove spacing
                    ) / NumColumns;
            }

            
            if (WidthHeightRatio <= 0)
            {
                WidthHeightRatio = layoutGroup.cellSize.x / layoutGroup.cellSize.y;
            }

            // update celle size config
            layoutGroup.cellSize = new Vector2(elemWidth, elemWidth / WidthHeightRatio);
        }
    }

    public class AchievementRewardsScroller : MObject
    {
        #region Members

        // Serialized Data
        [SerializeField] List<SGridLayoutItem> m_GridLayoutItems = new();

        // GameObjects & Components
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
            m_TabButtonsContainer   = Finder.Find(gameObject, "TabButtonsContainer");
            m_ScrollerContent       = Finder.Find(gameObject, "ScrollerContent");
            m_CancelButton          = Finder.FindComponent<Button>(gameObject, "CancelButton");

            // get dimension of the scroller
            UIHelper.GetSize(m_ScrollerContent, out float m_Width, out float _);
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

            // TODO : (?)   ===========================================================================
            // SGridLayoutItem selectedItem = m_GridLayoutItems.FirstOrDefault(item => item.ScrollItemType == achievementReward.ToString());
            // selectedItem.UpdateGridLayout(m_ScrollerContent);
            // TODO : (?)   ===========================================================================

            AchievementRewardScrollItemUI template = AssetLoader.Load<AchievementRewardScrollItemUI>(AssetLoader.c_AchievementsTemplatesPath + "AchievementRewardScrollItem");
            foreach (var value in ProfileCloudData.GetAchievementRewards(achievementReward, true))
            {
                var go = Instantiate(template, m_ScrollerContent.transform);
                go.Initialize(value, achievementReward);

                go.Button.onClick.AddListener(() => go.AchievementRewardUI.SetAsCurrent());
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