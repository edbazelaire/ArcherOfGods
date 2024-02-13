using Inventory;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu
{
    public class ChestsSectionUI : MonoBehaviour
    {
        #region Members

        const string c_ChestsContainer = "ChestsContainer";

        GameObject m_ChestsContainer;
        GameObject m_TempalteChestItem;
        List<TemplateChestItem> m_ChestItems;

        #endregion


        #region Init & End

        private void Awake()
        {
            m_ChestsContainer = Finder.Find(gameObject, c_ChestsContainer);
            m_TempalteChestItem = AssetLoader.Load<GameObject>("TemplateChestItem", AssetLoader.c_MainTab);

            SetupChestItems();
        }

        #endregion


        #region GUI Manipulators

        void SetupChestItems()
        {
            m_ChestItems = new();
            UIHelper.CleanContent(m_ChestsContainer);

            for (int i = 0; i < InventoryManager.Chests.Length; i++)
            {
                TemplateChestItem chestItem = Instantiate(m_TempalteChestItem, m_ChestsContainer.transform).GetComponent<TemplateChestItem>();
                chestItem.Initialize(InventoryManager.Chests[i], i);
                m_ChestItems.Add(chestItem);
            }
        }

        #endregion

    }
}