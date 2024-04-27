using UnityEngine;

namespace Menu.MainMenu
{
    public class TabContent : MObject
    {
        #region Members

        protected TabButton   m_TabButton;
        protected bool        m_Activated;

        public TabButton TabButton => m_TabButton;

        #endregion


        #region Init & End

        /// <summary>
        /// Called when the tab is getting initialized, during registration by the "TabsManager"
        /// </summary>
        /// <param name="tabButton"></param>
        public virtual void Initialize(TabButton tabButton)
        {
            base.Initialize();
            m_TabButton = tabButton;
        }

        /// <summary>
        /// Called when the tab gets activated / deactivated as currently displayed tab
        /// </summary>
        /// <param name="activate"></param>
        public virtual void Activate(bool activate)
        {
            m_Activated = activate;
            m_TabButton?.Activate(activate);
        }

        /// <summary>
        /// Called on beeing un-registered from the "TabsManager"
        /// </summary>
        public virtual void UnRegister() { }

        #endregion


        #region Listeners


        #endregion
    }
}