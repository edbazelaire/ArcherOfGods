using Enums;
using Inventory;
using Save;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu
{
    public class CurrencyContainer : MonoBehaviour
    {
        #region Members

        [SerializeField] ECurrency m_Currency;

        Image       m_Icon;
        GameObject  m_CurrencyContainer;
        TMP_Text    m_TextValue;

        #endregion


        #region Init & End

        protected void Awake()
        {
            m_Icon = Finder.FindComponent<Image>(gameObject, "Icon");
            m_TextValue = Finder.FindComponent<TMP_Text>(gameObject, "Value");

            m_Icon.sprite = AssetLoader.LoadCurrencyIcon(m_Currency);
            m_TextValue.text = TextHandler.FormatNumericalString(InventoryManager.GetCurrency(m_Currency));

            InventoryCloudData.CurrencyChangedEvent += OnCurrencyChanged;
        }

        #endregion


        #region Listeners

        void OnCurrencyChanged(ECurrency currency, int amount = 0)
        {
            if (currency != m_Currency)
                return;

            m_TextValue.text = InventoryManager.GetCurrency(m_Currency).ToString();
        }

        #endregion
    }
}