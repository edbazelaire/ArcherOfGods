using Data.GameManagement;
using System.Collections.Generic;

namespace Menu.MainMenu
{
    public class ChestsTabContent : ShopSubTabContent
    {
        #region Init & End

        public override void Initialize(TabButton tabButton)
        {
            base.Initialize(tabButton);

            m_Scroller.Initialize(new List<SShopData>[] { ShopManagementData.BundleShopData });
        }

        #endregion
    }
}