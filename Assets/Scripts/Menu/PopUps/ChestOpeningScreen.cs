using Data;
using Enums;
using Game.Managers;
using Inventory;
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
        SpriteRenderer      m_ChestPreview;
        Animator            m_ChestAnimator;
        GameObject          m_RewardDisplayContainer;
        Image               m_RewardIcon;
        TMP_Text            m_RewardQty;
        TMP_Text            m_RewardTitle;
        TMP_Text            m_Level;
        GameObject          m_QuantityBarContainer;
        Image               m_QuantityFill;
        TMP_Text            m_QuantityCounter;

        // SPECIFIC DATA
        ChestRewardData m_ChestRewardData;
        int m_RewardIndex = 0;
        Dictionary<EReward, object> m_Rewards;

        #endregion


        #region Constructor

        public ChestOpeningScreen() : base(EPopUpState.ChestOpeningScreen) { }

        public void Initialize(EChestType chestType)
        {
            m_ChestRewardData = ItemLoader.GetChestRewardData(chestType);
            m_RewardIndex = 0;

            base.Initialize();
        }

        #endregion


        #region Enter & Exit

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            SetupChestUI();
            SetupRewardsUI();
        }

        #endregion


        #region GUI Manipulators

        void SetupChestUI()
        {
            // setup game objects
            m_ChestContainer        = Finder.Find(gameObject, c_ChestContainer);
            m_ChestPreview          = Finder.FindComponent<SpriteRenderer>(m_ChestContainer, c_ChestPreview);
            m_ChestAnimator         = m_ChestPreview.GetComponent<Animator>();

            // setup image of the chest and idle animation
            m_ChestPreview.sprite   = m_ChestRewardData.Image;
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

        IEnumerator OpenChest()
        {
            m_ChestAnimator.Play(CHEST_OPENING_ANIMATION);

            // wait for the animation to start
            while (! m_ChestAnimator.GetCurrentAnimatorStateInfo(0).IsName(CHEST_OPENING_ANIMATION))
            {
                yield return null;
            }

            // wait for the end of the animation
            while (m_ChestAnimator.GetCurrentAnimatorStateInfo(0).IsName(CHEST_OPENING_ANIMATION))
            {
                yield return null;
            }

            // hide chest / display rewards section
            m_ChestContainer.gameObject.SetActive(false);
            m_RewardDisplayContainer.gameObject.SetActive(true);

            // start displaying rewards
            DisplayNextReward();
        }

        void DisplayNextReward()
        {
            if (m_RewardIndex >= m_Rewards.Count)
                OnExit();

            Sprite icon = null;
            int nextRequestedValue = 1;
            int currentlyOwnValue = 1;
            int qty = 0;
            EReward reward = m_Rewards.Keys.ToList()[m_RewardIndex];
            switch (reward)
            {
                case (EReward.Golds):
                    // collect reward
                    qty = (int)m_Rewards[reward];
                    InventoryManager.AddGolds(qty);

                    // setup icon and values
                    icon = AssetLoader.LoadCurrencyIcon(reward);
                    currentlyOwnValue = InventoryManager.Golds;
                    nextRequestedValue = currentlyOwnValue;
                    m_Level.gameObject.SetActive(false);
                    break;

                default:
                    ErrorHandler.Error("Unset Reward : " + reward);
                    break;
            }

            m_RewardIcon.sprite         = icon;
            m_RewardQty.text            = qty.ToString();
            m_RewardTitle.text          = reward.ToString();
            m_QuantityCounter.text      = string.Format("{0} / {1}", currentlyOwnValue, nextRequestedValue);
            m_QuantityFill.fillAmount   = Mathf.Clamp(currentlyOwnValue / nextRequestedValue, 0, 1);

            m_RewardIndex++;
        }

        #endregion


        #region Listeners

        protected override void OnUIButton(string bname)
        {
            Debug.Log("Button clicked : " + bname);
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
            StartCoroutine(OpenChest());
        }

        protected override void OnTouch(GameObject gameObject)
        {
            base.OnTouch(gameObject);

            // if first reward was already shown : display the next one
            if(m_RewardIndex > 0) 
                DisplayNextReward();
        }

        #endregion


    }
}