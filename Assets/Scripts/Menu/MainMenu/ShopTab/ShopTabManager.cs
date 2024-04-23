using System;
using Tools;
using UnityEngine;

namespace Menu
{
    public enum EShopTab
    {
        ChestsTab,
        GoldsTab,
    }

    public class ShopTabManager: TabsManager
    {
        #region Members

        protected override Type m_TabEnumType { get; set; } = typeof(EShopTab);
        protected override Enum m_DefaultTab { get; set; } = EShopTab.ChestsTab;

        #endregion

    }
}
