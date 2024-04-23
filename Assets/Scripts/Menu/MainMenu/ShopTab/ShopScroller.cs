using Data.GameManagement;
using Menu.Common.Buttons;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu.MainMenu.ShopTab
{
    public class ShopScroller : MonoBehaviour
    {
        #region Members

        TemplateShopItemButton  m_TemplateShopItem;
        GameObject              m_ContentContainer;

        List<SShopData>[] m_ShopDataList;

        #endregion


        #region Init & End

        public void Initialize(List<SShopData>[] shopData)
        {
            m_TemplateShopItem = AssetLoader.LoadTemplateItem("ShopItem").GetComponent<TemplateShopItemButton>();
            m_ContentContainer = this.gameObject;
            m_ShopDataList = shopData;

            SetUpScroller();
        }

        #endregion


        #region GUI Manipulators

        void SetUpScroller()
        {
            UIHelper.CleanContent(m_ContentContainer);

            if (m_TemplateShopItem == null)
            {
                ErrorHandler.Error("Template (TemplateShopItem) is null - skip");
                return;
            }

            foreach (List<SShopData> dataList in m_ShopDataList)
            {
                foreach (SShopData data in dataList)
                {
                    var shopItem = Instantiate(m_TemplateShopItem, m_ContentContainer.transform);
                    shopItem.Initialize(data.Name, data.Icon, data.Currency, data.Cost, data.Rewards);
                }
            }
        }

        #endregion
    }
}