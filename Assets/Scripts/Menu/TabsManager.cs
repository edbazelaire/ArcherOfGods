using Menu.MainMenu;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu
{
    public class TabsManager : MonoBehaviour
    {
        #region Members

        // =======================================================================================
        // GameObjects & Components
        protected const string      c_TabButtonsContainer       = "TabButtonsContainer";
        protected const string      c_TabButtonSuffix           = "Button";
        protected const string      c_TabsContainer             = "TabsContainer";
        protected const string      c_TabsContainerContent      = "Content";

        /// <summary> </summary>
        protected GameObject        m_TabButtonsContainer;
        /// <summary> </summary>
        protected GameObject        m_TabsContainer;
        /// <summary> </summary>
        protected GameObject        m_TabsContainerContent;

        // =======================================================================================
        // Data
        /// <summary> enum of all the tabs </summary>
        protected Type m_TabEnumType { get; set; } = null;

        /// <summary> dict of all Tabs tabs linked to their enum value </summary>
        protected Dictionary<Enum, TabContent> m_Tabs;
        /// <summary> default tab displayed on entering the menu </summary>
        protected Enum m_DefaultTab;
        /// <summary> current tab displayed </summary>
        protected Enum m_CurrentTab = null;

        /// <summary> tab content currently displayed </summary>
        protected TabContent m_CurrentTabContent => m_CurrentTab != null ? m_Tabs[m_CurrentTab] : null;

        #endregion


        #region Init & End

        private void Awake()
        {
            m_TabsContainer = Finder.Find(gameObject, c_TabsContainer);
            m_TabsContainerContent = Finder.Find(m_TabsContainer, c_TabsContainerContent);

            RegisterTabs();
        }

        #endregion


        #region Tabs Management

        /// <summary>
        /// Register and init all TabsButton and TabsContent
        /// </summary>
        protected virtual void RegisterTabs()
        {
            m_Tabs = new Dictionary<Enum, TabContent>();
            if (m_TabEnumType == null)
            {
                ErrorHandler.FatalError("Enum of tabs was not provided for " + name + " : set m_TabEnumType");
                return;
            }

            foreach (Enum tab in Enum.GetValues(m_TabEnumType))
            {
                RegisterTab(tab);
            }

            SelectTab(m_DefaultTab, false);
        }

        /// <summary>
        /// Register & Init the requested tab
        /// </summary>
        /// <param name="tab"></param>
        protected virtual void RegisterTab(Enum tab)
        {
            var tabObject = Finder.FindComponent<TabContent>(m_TabsContainerContent, tab.ToString());
            m_Tabs.Add(tab, tabObject);

            // find tab button
            TabButton tabButton = Finder.FindComponent<TabButton>(m_TabButtonsContainer, tab.ToString() + c_TabButtonSuffix);
            tabObject.Initialize(tabButton);
        }

        /// <summary>
        /// Select and Activate the requested tab
        /// </summary>
        /// <param name="tabIndex"></param>
        /// <param name="withAnim"></param>
        protected virtual void SelectTab(Enum tabIndex, bool withAnim = true)
        {
            if (m_CurrentTab == tabIndex)
                return;

            // deactivate current tab window
            m_CurrentTabContent?.Activate(false);

            // set new current tab
            m_CurrentTab = tabIndex;

            // adjust position of the new tab to match the viewport
            DisplayCurrentTab(withAnim);

            // activate new window tab
            m_CurrentTabContent.Activate(true);
        }

        /// <summary>
        /// Display the currently selected tab
        /// </summary>
        /// <param name="withAnim"></param>
        protected virtual void DisplayCurrentTab(bool withAnim = true)
        {

        }

        #endregion
    }
}