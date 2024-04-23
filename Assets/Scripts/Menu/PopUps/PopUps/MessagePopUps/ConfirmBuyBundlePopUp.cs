using Menu.Common.Displayers;
using Tools;

namespace Menu.PopUps
{
    public class ConfirmBuyBundlePopUp : ConfirmBuyPopUp
    {
        #region Members
       
        // GameObjects & Components
        RewardsDisplayer    m_RewardsDisplayer;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_RewardsDisplayer = Finder.FindComponent<RewardsDisplayer>(m_WindowContent, "RewardsDisplayer");
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            m_RewardsDisplayer.Initialize(m_RewardsData);
        }

        #endregion
    }
}