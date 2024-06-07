using Assets;
using Assets.Scripts.Menu.Common.Buttons.SubButtons;
using Data.GameManagement;
using Enums;
using Inventory;
using System;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class ConfirmBuyPopUp : MessagePopUp
    {
        #region Members

        // Data
        protected SPriceData        m_PriceData;
        protected SRewardsData      m_RewardsData;

        // GameObjects & Components
        protected Button            m_BuyButton;
        protected PriceDisplay      m_BuyButtonDisplay;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_BuyButton = Finder.FindComponent<Button>(m_Buttons, "BuyButton");
            m_BuyButtonDisplay = Finder.FindComponent<PriceDisplay>(m_BuyButton.gameObject);
        }

        public void Initialize(SPriceData priceData, SRewardsData rewardsData, Action onValidate, Action onCancel)
        {
            base.Initialize(GetMessage(), "Confirm Buy", onValidate, onCancel);
            m_PriceData = priceData;
            m_RewardsData = rewardsData;
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            m_BuyButtonDisplay.Initialize(m_PriceData);
        }

        #endregion


        #region Helpers

        protected virtual string GetMessage()
        {
            return "";

            // TODO : remove ?
            //return TextLocalizer.LocalizeText("Do you want to buy this item ?");
        }

        #endregion


        #region Listeners

        protected override void OnUIButton(string bname)
        {
            switch (bname)
            {
                case ("BuyButton"):
                    OnBuyClicked();
                    break;

                default:
                    base.OnUIButton(bname);
                    break;
            }

        }

        protected virtual void OnBuyClicked()
        {
            if (! InventoryManager.CanBuy(m_PriceData.Price, m_PriceData.Currency))
            {
                // TODO : CAN'T BUY ANIMATION & TEXT
                Debug.Log("UNABLE TO BUY");
                return;
            }

            InventoryManager.Spend(m_PriceData.Price, m_PriceData.Currency, "Shop");

            OnValidateButton();

            Exit();
        }

        protected virtual void OnPurchaseCompleted()
        {
            Main.DisplayRewards(m_RewardsData, ERewardContext.Shop);
        }

        #endregion
    }
}