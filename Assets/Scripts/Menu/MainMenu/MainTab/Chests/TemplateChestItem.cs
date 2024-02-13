using Game.Managers;
using Inventory;
using Save;
using System;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace Menu
{
    enum EChestButtonState
    {
        Empty,
        Locked,
        Ready
    }

    public class TemplateChestItem : MonoBehaviour
    {
        #region MyRegion

        const string c_EmptySlot                = "EmptySlot";
        const string c_ChestPreviewContainer    = "ChestPreviewContainer";
        const string c_ChestPreview             = "ChestPreview";
        const string c_ChestTimer               = "ChestTimer";

        Button m_Button;
        GameObject m_EmptySlot;
        GameObject m_ChestPreviewContainer;
        SpriteRenderer m_ChestPreview;
        TMP_Text m_ChestTimer;
        Animator m_Animator;

        ChestData m_ChestData       = null;
        int m_Index                 = -1;
        EChestButtonState m_State   = EChestButtonState.Empty;

        #endregion


        #region Init & End

        private void Awake()
        {
            m_Button                    = Finder.FindComponent<Button>(gameObject);
            m_EmptySlot                 = Finder.Find(gameObject, c_EmptySlot);
            m_ChestPreviewContainer     = Finder.Find(gameObject, c_ChestPreviewContainer);
            m_ChestPreview              = Finder.FindComponent<SpriteRenderer>(m_ChestPreviewContainer, c_ChestPreview);
            m_Animator                  = Finder.FindComponent<Animator>(m_ChestPreview.gameObject);
            m_ChestTimer                = Finder.FindComponent<TMP_Text>(m_ChestPreviewContainer, c_ChestTimer);

            // LISTENERS
            InventoryManager.ChestsAddedEvent += OnChestAdded;
            m_Button.onClick.AddListener(OnButtonClicked);
        }

        public void Initialize(ChestData chestData, int index)
        {
            m_Index = index;
            SetChestData(chestData);
        }

        public void OnDestroy()
        {
            if (m_Button != null)
                m_Button.onClick.RemoveAllListeners();

            InventoryManager.ChestsAddedEvent -= OnChestAdded;
        }

        #endregion


        #region Updates

        private void Update()
        {
            if (m_ChestData == null)
                return;

            RefreshCounter();
        }

        #endregion


        #region GUI Manipulators

        /// <summary>
        /// Setup new ChestData and refresh UI to match it
        /// </summary>
        /// <param name="chestData"></param>
        public void SetChestData(ChestData chestData)
        {
            m_ChestData = chestData;
            UpdateState();
        }

        /// <summary>
        /// Refresh UI to match the current ChestData
        /// </summary>
        void RefreshUI()
        {
            Activate(m_ChestData != null);

            if (m_ChestData == null)
                return;

            m_ChestPreview.sprite = ItemLoader.GetChestRewardData(m_ChestData.ChestType).Image;
        }

        /// <summary>
        /// Activate / Deactivate button depending on having chest data
        /// </summary>
        /// <param name="activate"></param>
        void Activate(bool activate)
        {
            m_EmptySlot.SetActive(! activate);
            m_ChestPreviewContainer.SetActive(activate);
            m_Button.interactable = activate;
        }

        /// <summary>
        /// Refresh the display of the counter
        /// </summary>
        void RefreshCounter()
        {
            if (m_State != EChestButtonState.Locked) 
                return;

            if (UnlockedIn <= 0)
                SetState(EChestButtonState.Ready);
            else
                m_ChestTimer.text = TextLocalizer.GetAsCounter(UnlockedIn);
            return;
        }

        #endregion


        #region State Management


        /// <summary>
        /// Set state depending on context
        /// </summary>
        void UpdateState()
        {
            if (m_ChestData is null)
            {
                SetState(EChestButtonState.Empty);
                return;
            }

            if (UnlockedIn > 0)
                SetState(EChestButtonState.Locked);
            else 
                SetState(EChestButtonState.Ready);
        }

        /// <summary>
        /// Change the State and Adapdt UI depending on it
        /// </summary>
        /// <param name="state"></param>
        void SetState(EChestButtonState state)
        {
            m_State = state;
            switch (state)
            {
                case EChestButtonState.Empty:
                    break;

                case EChestButtonState.Locked:
                    break;

                case EChestButtonState.Ready:
                    m_ChestTimer.text = TextLocalizer.LocalizeText("Ready");
                    m_Animator.SetTrigger("Ready");
                    break;
            }

            RefreshUI();
        }

        int UnlockedIn => (int)(m_ChestData.UnlockedAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        #endregion


        #region Listeners

        void OnButtonClicked()
        {
            if (m_State != EChestButtonState.Ready)
                return;

            InventoryManager.CollectChest(m_Index);
            SetChestData(null);
        }

        void OnChestAdded(ChestData chestData, int index)
        {
            if (index != m_Index)
                return;

            SetChestData(chestData);
        }

        #endregion
    }
}