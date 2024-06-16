using Tools;

namespace Menu.MainMenu
{
    public class ProfileTab : MainMenuTabContent
    {
        #region Members

        ProfileDisplayBE m_ProfileDisplayBE;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_ProfileDisplayBE = Finder.FindComponent<ProfileDisplayBE>(gameObject, "ProfileDisplay");
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            m_ProfileDisplayBE.Initialize();
        }

        #endregion
    }
}