using Enums;
using Inventory;
using Save;
using System.Collections;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu
{
    public class HUD : OvMonoBehavior
    {
        const string c_GoldsContainer   = "GoldsContainer";
        const string c_GoldsValue       = "GoldsValue";

        GameObject m_GoldsContainer;
        TMP_Text m_GoldsValue;

        protected override void Start()
        {
            base.Start();

            m_GoldsContainer    = Finder.Find(gameObject, c_GoldsContainer);
            m_GoldsValue        = Finder.FindComponent<TMP_Text>(m_GoldsContainer, c_GoldsValue);

            RefreshUI();

            // LISTENERS
            InventoryCloudData.GoldChangedEvent += RefreshUI;

            AddTest();
        }

        void AddTest()
        {
            var testButton = Finder.FindComponent<Button>(gameObject, "TestButton");
            testButton.onClick.AddListener(() => { InventoryManager.AddChest(InventoryManager.CreateRandomChest()); } );
        }

        #region GUI Manipulators

        void RefreshUI(int golds = 0)
        {
            m_GoldsValue.text = InventoryManager.Golds.ToString();
        }

        #endregion
    }
}