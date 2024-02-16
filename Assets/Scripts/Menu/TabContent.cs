namespace Menu.MainMenu
{
    public class TabContent : OvMonoBehavior
    {
        #region Members

        protected TabButton   m_TabButton;
        protected bool        m_Activated;

        #endregion

        public virtual void Initialize(TabButton tabButton)
        {
            
        }
        
        public virtual void Activate(bool activate)
        {
            m_Activated = activate;
            m_TabButton?.Activate(activate);
        }
    }
}