using Assets.Scripts.Menu.MainMenu.MainTab.Chests;
using Data;
using Enums;
using Game.Managers;
using Inventory;
using Save;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class ChestOpeningScreen : OverlayScreen
    {
        #region Members

        const string        CHEST_OPENING_ANIMATION     = "ChestJump";

        const string        c_ChestContainer            = "ChestContainer";
        const string        c_ChestPreview              = "ChestPreview";
        const string        c_RewardDisplayContainer    = "RewardDisplayContainer";

        GameObject          m_ChestContainer;
        ChestUI             m_ChestUI;
        GameObject          m_RewardDisplayContainer;
        Image               m_RewardIcon;
        TMP_Text            m_RewardQty;
        TMP_Text            m_RewardTitle;
        TMP_Text            m_Level;
        GameObject          m_QuantityBarContainer;
        Image               m_QuantityFill;
        TMP_Text            m_QuantityCounter;

        // SPECIFIC DATA
        ChestRewardData     m_ChestRewardData;
        int                 m_ChestIndex;   
        int                 m_RewardIndex       = 0;
        List<SReward>       m_Rewards           = new();

        #endregion


        #region Constructor

        public ChestOpeningScreen() : base(EPopUpState.ChestOpeningScreen) { }

        public void Initialize(EChestType chestType, int chestIndex = -1)
        {
            m_ChestRewardData   = ItemLoader.GetChestRewardData(chestType);
            m_ChestIndex        = chestIndex;  
            m_RewardIndex       = 0;

            base.Initialize();
        }

        #endregion


        #region Enter & Exit

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            SetupChestUI();
            SetupRewardsUI();

            // listeners
            m_ChestUI.OpenAnimationEndedEvent += OnOpenAnimationEnded;

        }

        protected override void OnExit()
        {
            m_ChestUI.OpenAnimationEndedEvent -= OnOpenAnimationEnded;

            base.OnExit();
        }

        #endregion


        #region GUI Manipulators

        void SetupChestUI()
        {
            // setup game objects
            m_ChestContainer        = Finder.Find(gameObject, c_ChestContainer);

            // instantiate chest prefab
            m_ChestUI               = m_ChestRewardData.Instantiate(m_ChestContainer);
            m_ChestUI.ActivateIdle(true);
        }

        void SetupRewardsUI()
        {
            m_RewardDisplayContainer    = Finder.Find(gameObject, c_RewardDisplayContainer);
            m_RewardIcon                = Finder.FindComponent<Image>(m_RewardDisplayContainer, "RewardIcon");
            m_RewardQty                 = Finder.FindComponent<TMP_Text>(m_RewardDisplayContainer, "RewardQty");
            m_RewardTitle               = Finder.FindComponent<TMP_Text>(m_RewardDisplayContainer, "RewardTitle");
            m_Level                     = Finder.FindComponent<TMP_Text>(m_RewardDisplayContainer, "Level");
            m_QuantityBarContainer      = Finder.Find(gameObject, "QuantityBarContainer");
            m_QuantityFill              = Finder.FindComponent<Image>(m_QuantityBarContainer, "Fill");
            m_QuantityCounter           = Finder.FindComponent<TMP_Text>(m_QuantityBarContainer, "Counter");

            // hide before displaying rewards
            m_RewardDisplayContainer.SetActive(false);
        }

        #endregion


        #region Rewards

        void OpenChest()
        {
            m_ChestUI.ActivateOpen(true);
        }

        void DisplayNextReward()
        {
            if (m_RewardIndex >= m_Rewards.Count)
            {
                OnExit();
                return;
            }

            string title = "";
            Sprite icon = null;
            int nextRequestedValue = 1;
            int currentlyOwnValue = 1;
            int level = 0;

            SReward reward = m_Rewards[m_RewardIndex];
            int qty = reward.Qty;

            if (InventoryManager.CURRENCIES.Contains(reward.RewardType))
            {
                // setup icon and values
                title = reward.RewardType.ToString();
                icon = AssetLoader.LoadCurrencyIcon(reward.RewardType);
                currentlyOwnValue = InventoryManager.GetCurrency(reward.RewardType) - qty;
                nextRequestedValue = currentlyOwnValue;
                m_Level.gameObject.SetActive(false);
            } 
            else
            {
                switch (reward.RewardType)
                {
                    case (ERewardType.Spell):
                        // get data from cloud manager
                        ESpell spell = (ESpell)reward.Metadata[SReward.METADATA_KEY_SPELL_TYPE];
                        SSpellCloudData spellCloudData = InventoryManager.GetSpellData(spell);

                        // setup infos
                        title = spell.ToString();                       // title is the name of the spell
                        icon = AssetLoader.LoadSpellIcon(spell);        // load icon from ressources
                        nextRequestedValue = 1;                         // TODO     -------------------------------------------------------
                        currentlyOwnValue = spellCloudData.Qty - qty;   // quantity is collected before display so retrieve the qty to the current value to know how much the player used to own
                        level = spellCloudData.Level;                   // get level from cloud data
                        m_Level.gameObject.SetActive(true);             // make sure that level TMP is active
                        break;

                    default:
                        ErrorHandler.Error("Unset Reward : " + reward);
                        break;
                }
            
            }

            m_RewardIcon.sprite         = icon;
            m_RewardQty.text            = qty.ToString();
            m_RewardTitle.text          = title;
            m_QuantityCounter.text      = string.Format("{0} / {1}", currentlyOwnValue, nextRequestedValue);
            m_QuantityFill.fillAmount   = Mathf.Clamp(currentlyOwnValue / nextRequestedValue, 0, 1);

            if (m_Level.isActiveAndEnabled) 
            {
                m_Level.text = TextLocalizer.LocalizeText("Level ") + level.ToString();
            }

            m_RewardIndex++;
        }

        #endregion


        #region Listeners

        protected override void OnUIButton(string bname)
        {
            switch(bname) 
            {
                case (c_ChestContainer):
                    OnChestClicked();
                    break;

                default:
                    base.OnUIButton(bname);
                    break;
            }
        }

        void OnChestClicked()
        {
            // set button not interactable
            m_ChestContainer.GetComponent<Button>().interactable = false;

            // get list of rewards
            m_Rewards = m_ChestRewardData.GetRewards();

            // animate chest opening and display rewards
            OpenChest();
        }

        protected override void OnTouch(GameObject gameObject)
        {
            base.OnTouch(gameObject);

            // if first reward was already shown : display the next one
            if(m_RewardIndex > 0) 
                DisplayNextReward();
        }

        void OnOpenAnimationEnded()
        {
            // hide chest / display rewards section
            m_ChestContainer.gameObject.SetActive(false);
            m_RewardDisplayContainer.gameObject.SetActive(true);

            // collect chest rewards and remove it from database if is in 
            InventoryManager.CollectRewards(m_Rewards);
            if (m_ChestIndex >= 0)
                InventoryManager.RemoveChestAtIndex(m_ChestIndex);

            // start displaying rewards
            DisplayNextReward();
        }

        #endregion


    }
}