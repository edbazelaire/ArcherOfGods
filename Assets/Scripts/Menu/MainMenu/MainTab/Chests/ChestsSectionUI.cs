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
        GameObject m_ChestUnlockPrefab;
        List<ChestUnlock> m_ChestItems;

        #endregion


        #region Init & End

        private void Awake()
        {
            m_ChestsContainer = Finder.Find(gameObject, c_ChestsContainer);
            m_ChestUnlockPrefab = AssetLoader.Load<GameObject>("ChestUnlock", AssetLoader.c_MainTabPath);

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
                ChestUnlock chestItem = Instantiate(m_ChestUnlockPrefab, m_ChestsContainer.transform).GetComponent<ChestUnlock>();
                chestItem.Initialize(i);
                m_ChestItems.Add(chestItem);
            }
        }

        #endregion

    }
}