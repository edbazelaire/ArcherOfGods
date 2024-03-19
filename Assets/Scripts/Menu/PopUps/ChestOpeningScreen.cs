using Assets.Scripts.Menu.Common.Buttons.TemplateItemButtons;
using Assets.Scripts.Menu.MainMenu.MainTab.Chests;
using Data;
using Enums;
using Game.Managers;
using Inventory;
using Menu.Common;
using Menu.Common.Buttons;
using Menu.MainMenu;
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

        const string        c_ChestContainer            = "ChestContainer";
        const string        c_RewardDisplayContainer    = "RewardDisplayContainer";

        GameObject          m_ChestContainer;
        ChestUI             m_ChestUI;
        GameObject          m_RewardDisplayContainer;
        GameObject          m_RewardIconSection;
        TMP_Text            m_RewardTitle;
        CollectionFillBar   m_CollectionFillBar;
        TMP_Text            m_CollectionQty;

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
            m_RewardIconSection         = Finder.Find(m_RewardDisplayContainer, "RewardIconSection");
            m_RewardTitle               = Finder.FindComponent<TMP_Text>(m_RewardDisplayContainer, "RewardTitle");
            m_CollectionFillBar         = Finder.FindComponent<CollectionFillBar>(m_RewardDisplayContainer, "CollectionFillbar");
            m_CollectionQty             = Finder.FindComponent<TMP_Text>(m_RewardDisplayContainer, "CollectionQty");

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
            int currentlyOwnValue = 1;
            SReward reward = m_Rewards[m_RewardIndex];
            int qty = reward.Qty;
            int maxCollection = 1;

            // init default template and clean previous content
            UIHelper.CleanContent(m_RewardIconSection);
            TemplateItemButton template = default;
            
            if (InventoryManager.CURRENCIES.Contains(reward.RewardType))
            {
                template = Instantiate(AssetLoader.LoadTemplateItem("CurrencyIcon"), m_RewardIconSection.transform).GetComponent<TemplateCurrencyIcon>();
                (template as TemplateCurrencyIcon).Initialize(reward.RewardType);

                // setup icon and values
                title = reward.RewardType.ToString();
                currentlyOwnValue = InventoryManager.GetCurrency(reward.RewardType);
                maxCollection = currentlyOwnValue + qty;
            } 
            else
            {
                switch (reward.RewardType)
                {
                    case (ERewardType.Spell):
                        ESpell spell = (ESpell)reward.Metadata[SReward.METADATA_KEY_SPELL_TYPE];
                        template = Instantiate(AssetLoader.LoadTemplateItem("SpellItem"), m_RewardIconSection.transform).GetComponent<TemplateSpellItemUI>();
                        (template as TemplateSpellItemUI).Initialize(spell);

                        // get data from cloud manager
                        SSpellCloudData spellCloudData = InventoryManager.GetSpellData(spell);

                        // setup infos
                        title = spell.ToString();                       // title is the name of the spell
                        currentlyOwnValue = spellCloudData.Qty;         // quantity is collected before display so retrieve the qty to the current value to know how much the player used to own
                        maxCollection = SpellLoader.GetSpellLevelData(spell).RequiredCards;
                        break;

                    default:
                        ErrorHandler.Error("Unset Reward : " + reward);
                        break;
                }
            
            }

            // remove button and collection fillbar from item
            template.AsIconOnly();

            m_CollectionQty.text        = "+ " + qty.ToString();
            m_RewardTitle.text          = title;

            // -- setup collection fill bar
            m_CollectionFillBar.Initialize(currentlyOwnValue, maxCollection);
            m_CollectionFillBar.AddCollectionAnimation(qty);
            
            // add reward to collection of rewards
            InventoryManager.CollectReward(reward);

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

            if (m_ChestIndex >= 0)
                InventoryManager.RemoveChestAtIndex(m_ChestIndex);

            // start displaying rewards
            DisplayNextReward();
        }

        #endregion


    }
}