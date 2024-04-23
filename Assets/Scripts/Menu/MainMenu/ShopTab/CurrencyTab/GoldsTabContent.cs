using Data.GameManagement;
using Menu.MainMenu.ShopTab;
using System.Collections.Generic;
using Tools;

namespace Menu.MainMenu
{
    public class GoldsTabContent : ShopSubTabContent
    {
        #region Init & End

        public override void Initialize(TabButton tabButton)
        {
            base.Initialize(tabButton);

            m_Scroller.Initialize(new List<SShopData>[] { ShopManagementData.GoldsShopData, ShopManagementData.GemsShopData });
        }

        #endregion
    }
}