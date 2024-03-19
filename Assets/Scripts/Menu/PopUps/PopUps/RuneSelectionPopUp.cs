using Data;
using Enums;
using Game.Managers;
using Game.Spells;
using Inventory;
using Menu.Common.Buttons;
using Menu.Common.Infos;
using Menu.MainMenu;
using Save;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class RuneSelectionPopUp : PopUp
    {
        #region Members

        // =========================================================================================
        // Data
        ERune                               m_CurrentRune;
        GameObject                          m_TemplateRuneButton;

        // =========================================================================================
        // GameObjects & Components
        GameObject                          m_RunesContent;
        TemplateRuneButton                  m_CurrentRuneItem;
        TMP_Text                            m_CurrentRuneTitle;
        TMP_Text                            m_CurrentRuneDescription;
        Button                              m_UpgradeButton;
        Button                              m_UseButton;
        TMP_Text                            m_CostText;

        // =========================================================================================
        // Dependent Members
        bool m_CanUpgrade => true;
        bool m_IsMaxedLevel => false;


        #endregion


        #region Constructor

        public RuneSelectionPopUp() : base(EPopUpState.RuneSelectionPopUp) { }

        #endregion


        #region Init & End

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            // -- scroller
            var rightSection            = Finder.Find(m_WindowContent, "RightSection");
            m_RunesContent              = Finder.Find(rightSection, "Content");
            m_TemplateRuneButton        = AssetLoader.LoadTemplateItem("RuneButton");

            // -- left section
            var leftSection             = Finder.Find(m_WindowContent, "LeftSection");
            m_CurrentRuneItem           = Finder.FindComponent<TemplateRuneButton>(leftSection, "CurrentRune");
            m_CurrentRuneTitle          = Finder.FindComponent<TMP_Text>(leftSection, "RuneTitle");
            m_CurrentRuneDescription    = Finder.FindComponent<TMP_Text>(leftSection, "Description");
            m_CurrentRuneItem.GetComponent<Button>().interactable = false;      // deactivate button of "current rune"
            m_CurrentRuneItem.Initialize(CharacterBuildsCloudData.CurrentRune);

            // -- buttons
            m_UseButton = Finder.FindComponent<Button>(m_Buttons, "UseSubButton");
            m_UpgradeButton = Finder.FindComponent<Button>(m_Buttons, "UpgradeSubButton");
            m_CostText = Finder.FindComponent<TMP_Text>(m_UpgradeButton.gameObject, "CostText");

            // set ui of current rune
            RefreshCurrentRuneSelection(CharacterBuildsCloudData.CurrentRune);

            // initialize scroller of Runes
            SetUpRuneScroller();

            // setup listeners
            TemplateRuneButton.ButtonClickedEvent += RefreshCurrentRuneSelection;
        }

        protected override void OnExit()
        {
            base.OnExit();

            TemplateRuneButton.ButtonClickedEvent -= RefreshCurrentRuneSelection;
        }

        #endregion


        #region UIManipulators

        void RefreshCurrentRuneSelection(ERune rune)
        {
            m_CurrentRune = rune;
            m_CurrentRuneItem.RefreshRune(rune);
            m_CurrentRuneTitle.text = rune.ToString() + " Rune";
            m_CurrentRuneDescription.text = rune.ToString() + " : TODO - Add Description";

            RefreshUpgradeButtonUI();
        }

        void SetUpRuneScroller()
        {
            UIHelper.CleanContent(m_RunesContent);
            foreach (ERune rune in Enum.GetValues(typeof(ERune)))
            {
                var runeItem = Instantiate(m_TemplateRuneButton, m_RunesContent.transform).GetComponent<TemplateRuneButton>();
                runeItem.Initialize(rune);
            }
        }

        /// <summary>
        /// Refresh UI to display if the UpgradeButton can be use or not
        /// </summary>
        void RefreshUpgradeButtonUI()
        {
            if (m_IsMaxedLevel)
            {
                m_UpgradeButton.gameObject.SetActive(false);
                return;
            }

            var buttonImage = Finder.FindComponent<Image>(m_UpgradeButton.gameObject);

            m_UpgradeButton.interactable = m_CanUpgrade;
            buttonImage.color = m_CanUpgrade ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            m_CostText.text = "0";
        }

        #endregion


        #region Listeners

        protected override void OnUIButton(string bname)
        {
            switch (bname)
            {
                case "UpgradeSubButton":
                    OnUpgrade();
                    return;

                case "UseSubButton":
                    OnUse();
                    return;

                default:
                    base.OnUIButton(bname);
                    return;
            }
        }

        void OnUpgrade()
        {
            Debug.Log("UPGRADE");
        }

        void OnUse()
        {
            CharacterBuildsCloudData.SetCurrentRune(m_CurrentRune);
            OnExit();
        }

        #endregion
    }
}