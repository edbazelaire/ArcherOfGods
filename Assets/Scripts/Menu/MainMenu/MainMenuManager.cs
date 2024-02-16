using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu.MainMenu
{
    public enum EMainMenuTabs
    {
        MainTab,
        InventoryTab,
    }

    public class MainMenuManager : TabsManager
    {
        #region Members

        /// <summary> scroll percentage of the window to consider going to the next tab </summary>
        [SerializeField] float m_NewTabScrollThreshold = 0.15f;
        /// <summary> time that the scroll animation will take to move an entire tab window </summary>
        [SerializeField] float m_ScrollTime = 0.5f;

        /// <summary> type of enum used for the tabs </summary>
        protected Type m_TabEnumType { get; set; } = typeof(EMainMenuTabs);

        /// <summary> coroutine of the scroll end animation </summary>
        Coroutine m_ScrollCoroutine;

        // =================================================================================================
        // ACCESSORS (casted values)
        public  MainMenuTabContent  CurrentTabWindow    => (MainMenuTabContent)m_CurrentTabContent;
        public  EMainMenuTabs?      CurrentTab          => m_CurrentTab != null ? (EMainMenuTabs)m_CurrentTab : null;

        #endregion


        #region Updates

        private void Update()
        {
            if (Input.GetMouseButtonUp(0) && m_TabsContainerContent != null)
            {
                OnScrollEnd();
            }

            if (Input.GetMouseButtonDown(0) && m_ScrollCoroutine != null)
            {
                StopCoroutine(m_ScrollCoroutine);
            }
        }

        #endregion


        #region Tabs Management

        protected override void RegisterTab(Enum tab)
        {
            base.RegisterTab(tab);

            // update size of the window to be sure it will match the screen size
            ((MainMenuTabContent)m_Tabs[tab]).SetWidth(Finder.FindComponent<RectTransform>(m_TabsContainer).rect.width);
        }

        protected override void DisplayCurrentTab(bool withAnim = true)
        {
            m_ScrollCoroutine = StartCoroutine(ResetCurrentTabPositionCoroutine(withAnim));
        }

        /// <summary>
        /// Animation of the scrolling reseting the position of the TabContainerContent to align the current tab window to the viewport
        /// </summary>
        /// <returns></returns>
        IEnumerator ResetCurrentTabPositionCoroutine(bool withAnim = true)
        {
            // -- size of the window
            float windowWidth = CurrentTabWindow.RectTransform.rect.width;
            float expectedPosition = -(int)CurrentTab.Value * windowWidth;
            float scrollSpeed =  windowWidth / m_ScrollTime;

            if (! withAnim)
            {
                m_TabsContainerContent.transform.localPosition = new Vector3(expectedPosition, m_TabsContainerContent.transform.localPosition.y, 1);
                yield break;
            }

            bool IsMovingLeft = expectedPosition < m_TabsContainerContent.transform.localPosition.x;
            Vector3 pos = m_TabsContainerContent.transform.localPosition;
            while (pos.x != expectedPosition)
            {
                float newPosition = pos.x + (IsMovingLeft ? -1 : 1) * Time.deltaTime * scrollSpeed;

                // check that new position does not exceed expected position
                if ((IsMovingLeft && newPosition < expectedPosition) || (!IsMovingLeft && newPosition > expectedPosition))
                    newPosition = expectedPosition;

                pos.x = newPosition;
                m_TabsContainerContent.transform.localPosition = pos;
                yield return null;
            }
        }

        #endregion


        #region Listeners

        void OnScrollEnd() 
        {
            if (m_CurrentTabContent == null)
            {
                ErrorHandler.Error("Current tab window not set");
                return;
            }

            // =============================================================================================================
            // CHECK : check if movement is enought to justify a tab change
            // -- size of the window
            float windowWidth = CurrentTabWindow.RectTransform.rect.width;
            // -- movement of the container relative to the current window displayed
            float movementX = m_TabsContainerContent.transform.localPosition.x + (int)CurrentTab.Value * windowWidth;

            // if not above threshold : reset the position of the window
            if (Math.Abs(movementX) / windowWidth < m_NewTabScrollThreshold)
            {
                Debug.Log("SCROLL THRESHOLD : Threshold not reached - resetting");
                DisplayCurrentTab();
                return;
            }

            // =============================================================================================================
            // GO TO NEXT TAB
            // check current tab moving left or right
            int nextTab = (int)(EMainMenuTabs)m_CurrentTab;
            if (movementX <= 0)
                nextTab++;
            else
                nextTab--;

            // if exists select the new tab, otherwise the scroller will handle repositionning the tab window
            if (Enum.IsDefined(typeof(EMainMenuTabs), nextTab))
                SelectTab((EMainMenuTabs)nextTab);
            
        }

        #endregion

    }
}