using Assets;
using Enums;
using Game.Managers;
using Inventory;
using Menu.Common;
using Menu.Common.Buttons;
using Save;
using System;
using System.Linq;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MainMenu
{
    enum EButtonState
    {
        Locked,
        Normal,
        Updatable
    }

    public class TemplateSpellItemUI : TemplateSpellButton

    {
        #region Members

        // ========================================================================================
        // GameObjects & Components
        GameObject          m_SubButtons;
        Button              m_UseButton;
        Button              m_RemoveButton;
        Button              m_InfosButton;
        Button              m_UpgradeButton;
        Image               m_UpgradeButtonImage;
        TMP_Text            m_UpgradeButtonCostText;

        // -- collection bar
        CollectionFillBar   m_CollectionFillBar;

        // ========================================================================================
        // SpellData
        bool m_IsLinked;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Border                = Finder.FindComponent<Image>(gameObject, "IconContainer");
            m_SubButtons            = Finder.Find(gameObject, "SubButtons");
            m_UseButton             = Finder.FindComponent<Button>(m_SubButtons, "UseSubButton");
            m_RemoveButton          = Finder.FindComponent<Button>(m_SubButtons, "RemoveSubButton");
            m_InfosButton           = Finder.FindComponent<Button>(m_SubButtons, "InfosSubButton");
            m_UpgradeButton         = Finder.FindComponent<Button>(m_SubButtons, "UpgradeSubButton");
            m_UpgradeButtonImage    = Finder.FindComponent<Image>(m_UpgradeButton.gameObject);
            m_UpgradeButtonCostText = Finder.FindComponent<TMP_Text>(m_UpgradeButton.gameObject, "CostText");

        }

        public void Initialize(ESpell spell, bool asIconOnly = false)
        {
            base.Initialize(InventoryManager.GetSpellData(spell));
            m_IsLinked = SpellLoader.GetSpellData(spell).Linked;

            // Setup UI
            // -- setup collection bar
            SetUpCollectionFillBar(!asIconOnly);

            // -- hide sub buttons
            m_SubButtons.SetActive(false);

            // check context of state and apply it
            UpdateState();

            // Listeners
            m_UseButton.onClick.AddListener(OnUseButtonClicked);
            m_RemoveButton.onClick.AddListener(OnRemoveButtonClicked);
            m_InfosButton.onClick.AddListener(OnInfosButtonClicked);   
            m_UpgradeButton.onClick.AddListener(OnUpgradeButtonClicked);   
            InventoryCloudData.SpellDataChangedEvent += OnSpellDataChanged;
            InventoryManager.CharacterLeveledUpEvent += OnCharacterLeveledUp;
            TemplateSpellItemUI.ButtonClickedEvent += OnAnySpellItemButtonClicked;

            // remove extra features if this is only requested as icon
            AsIconOnly(asIconOnly);
        }

        /// <summary>
        /// On destroy : remove all listeners
        /// </summary>
        protected override void OnDestroy()
        {
            if (m_Button == null)
                return;

            m_UseButton.onClick.RemoveAllListeners();
            m_RemoveButton.onClick.RemoveAllListeners();
            m_InfosButton.onClick.RemoveAllListeners();
            m_UpgradeButton.onClick.RemoveAllListeners();

            InventoryCloudData.SpellDataChangedEvent -= OnSpellDataChanged;
            InventoryManager.CharacterLeveledUpEvent -= OnCharacterLeveledUp;
            TemplateSpellItemUI.ButtonClickedEvent -= OnAnySpellItemButtonClicked;
        }

        #endregion



        #region GUI Manipulators

        public override void AsIconOnly(bool activate = true)
        {
            base.AsIconOnly(activate);

            m_CollectionFillBar?.gameObject.SetActive(! activate);
        }

        void SetUpCollectionFillBar(bool activate = true)
        {
            // find collection fill bar
            m_CollectionFillBar = Finder.FindComponent<CollectionFillBar>(gameObject, "CollectionFillBar", false);

            // there is no collection of linked spell
            if (SpellLoader.GetSpellData(m_SpellCloudData.Spell).Linked)
            {
                if (m_CollectionFillBar != null)
                    m_CollectionFillBar.gameObject.SetActive(false);
                return;
            } 

            // if not linked : check existence
            if (m_CollectionFillBar == null) 
            {
                ErrorHandler.Error("Unable to find CollectionFillBar for template spell item of " + m_SpellCloudData.Spell);
                return;
            }

            if (m_SpellCloudData.Level > 0)
                m_CollectionFillBar.Initialize(m_SpellCloudData.Qty, SpellLoader.GetSpellLevelData(m_SpellCloudData.Spell).RequiredCards);
            else
                m_CollectionFillBar.Initialize(0, 1);

            m_CollectionFillBar.gameObject.SetActive(activate);
        }

        /// <summary>
        /// Switch display between SubButtons and CollectionFillBar
        /// </summary>
        void ToggleSubButtons()
        {
            bool alreadyActivated = m_SubButtons.activeInHierarchy;
            m_SubButtons.SetActive(! alreadyActivated);

            RefreshSubButtonUI();
        }

        /// <summary>
        /// Force closing sub buttons
        /// </summary>
        void CloseSubButtons()
        {
            m_SubButtons.SetActive(false);
        }

        /// <summary>
        /// + Activate / Deactivate allowed sub buttons 
        /// + Handle states (interactable, color, ...) 
        /// + Refresh values (like cost) 
        /// </summary>
        void RefreshSubButtonUI()
        {
            // check active 
            if (!m_SubButtons.activeInHierarchy)
                return;

            // if linked : deactivate all sub buttons but Infos
            if (m_IsLinked)
            {
                m_UseButton.gameObject.SetActive(false);
                m_RemoveButton.gameObject.SetActive(false);
                m_UpgradeButton.gameObject.SetActive(false);
                m_InfosButton.gameObject.SetActive(true);
                return;
            }

            // USE & REMOVE BUTTONS
            m_UseButton.gameObject.SetActive(!IsInCurrentBuild);            // USE BUTTON : NOT in current build and NOT a linked spell (cant be added or removed)
            m_RemoveButton.gameObject.SetActive(IsInCurrentBuild);          // REMOVE BUTTON : in current build and NOT a linked spell

            // UPGRADE / INFOS BUTTONS
            if (m_State == EButtonState.Updatable)
            {
                // deactivate InfosButton
                m_InfosButton.gameObject.SetActive(false);

                // activate UpgradeButton
                m_UpgradeButton.gameObject.SetActive(true);
                int spellCost = SpellLoader.GetSpellLevelData(m_SpellCloudData.Spell).RequiredGolds;
                m_UpgradeButtonImage.color = InventoryManager.CanBuy(spellCost) ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                m_UpgradeButtonCostText.text = spellCost.ToString();
            } 
            else
            {
                // activate InfosButton
                m_InfosButton.gameObject.SetActive(true);
                // deactivate UpgradeButton
                m_UpgradeButton.gameObject.SetActive(false);
            }
        }

        #endregion


        #region State Management

        /// <summary>
        /// Check context to define which state the button is in - set state accordingly
        /// </summary>
        void UpdateState()
        {
            // linked spell are always in "Normal" state
            if (SpellLoader.GetSpellData(m_SpellCloudData.Spell).Linked)
            {
                SetState(EButtonState.Normal);
                return;
            }

            if (m_SpellCloudData.Level == 0)
            {
                SetState(EButtonState.Locked);
                return;
            }

            if (m_SpellCloudData.Level <= SpellLoader.SpellsManagementData.SpellLevelData.Count && m_SpellCloudData.Qty >= SpellLoader.GetSpellLevelData(m_SpellCloudData.Spell).RequiredCards)
            {
                SetState(EButtonState.Updatable);
                return;
            }

            SetState(EButtonState.Normal);
        }

        /// <summary>
        /// Set UI according to the provided state
        /// </summary>
        /// <param name="state"></param>
        protected override void SetState(EButtonState state)
        {
            base.SetState(state);

            switch (state)
            {
                case (EButtonState.Locked):
                    m_LevelValue.text = "";
                    break;

                case (EButtonState.Updatable):
                case (EButtonState.Normal):
                    m_LevelValue.text = string.Format(LEVEL_FORMAT, m_SpellCloudData.Level);
                    break;

                default:
                    ErrorHandler.Error("UnHandled state " + state);
                    break;
            }

            if (m_CollectionFillBar != null && m_CollectionFillBar.isActiveAndEnabled)
                m_CollectionFillBar.UpdateCollection(m_SpellCloudData.Qty, m_SpellCloudData.Level > 0 ? SpellLoader.GetSpellLevelData(m_SpellCloudData.Spell).RequiredCards : 1);
        }

        #endregion


        #region Listeners

        /// <summary>
        /// Action happening when the button is clicked on - depending on button context
        /// </summary>
        protected override void OnClick()
        {
            // fire the event
            ButtonClickedEvent?.Invoke(m_SpellCloudData.Spell);

            // behavior when the USE button was clicked and this is one of the current build spells
            if (CurrentBuildDisplayUI.CurrentSelectedCard != null && CharacterBuildsCloudData.CurrentBuild.Contains(m_SpellCloudData.Spell))
            {
                CurrentBuildDisplayUI.ReplaceSpell(m_SpellCloudData.Spell);
                return;
            }

            // normal behavior
            switch (m_State)
            {
                case EButtonState.Locked:
                    break;

                case EButtonState.Normal:
                case EButtonState.Updatable:
                    ToggleSubButtons();
                    break;
            }
        }

        /// <summary>
        /// When the use button is clicked : set spell to current build
        /// </summary>
        void OnUseButtonClicked()
        {
            ToggleSubButtons();

            // try to find empty slot
            if (CurrentBuildDisplayUI.UseFirstEmptySlot(m_SpellCloudData.Spell))
                return;

            // if no empty slot, set card as current selected
            CurrentBuildDisplayUI.SetCurrentSelectedCard(m_SpellCloudData.Spell);
        }

        /// <summary>
        /// When the remove button is clicked : remove spell from current build
        /// </summary>
        void OnRemoveButtonClicked()
        {
            ToggleSubButtons();

            // remove the spell from the current build display
            CurrentBuildDisplayUI.RemoveSpell(m_SpellCloudData.Spell);
            // display the spell item in the list of spell items
            SpellsTabContent.ShowSpellItem(m_SpellCloudData.Spell, true);
        }

        /// <summary>
        /// On clicking the "Info Button" : display info of the spell
        /// </summary>
        void OnInfosButtonClicked()
        {
            ToggleSubButtons();
            Main.SetPopUp(EPopUpState.SpellInfoPopUp, m_SpellCloudData.Spell, m_SpellCloudData.Level);
        }

        /// <summary>
        /// On clicking the "Upgrade Button" : upgrade the spell's level and close sub buttons
        /// </summary>
        void OnUpgradeButtonClicked()
        {
            ToggleSubButtons();
            Main.SetPopUp(EPopUpState.SpellInfoPopUp, m_SpellCloudData.Spell, m_SpellCloudData.Level);
        }

        /// <summary>
        /// When the value of this button's linked "SSpellCloudData" changes, reload it and apply changes to the UI
        /// </summary>
        /// <param name="spellCloudData"></param>
        void OnSpellDataChanged(SSpellCloudData spellCloudData)
        {
            if (spellCloudData.Spell != m_SpellCloudData.Spell)
                return;
            m_SpellCloudData = spellCloudData;
            UpdateState();
        }

        void OnCharacterLeveledUp(ECharacter character)
        {
            // this applies only for linked spell of the current selected character
            if (! m_IsLinked || character != CharacterBuildsCloudData.SelectedCharacter)
                return;

            // refresh spell cloud data
            m_SpellCloudData = InventoryManager.GetSpellData(m_SpellCloudData.Spell);
            UpdateState();
        }

        void OnAnySpellItemButtonClicked(ESpell spell)
        {
            if (spell != m_SpellCloudData.Spell) 
                CloseSubButtons();
        }

        #endregion


        #region Dependent Members

        bool IsInCurrentBuild => CharacterBuildsCloudData.CurrentBuild.Contains(m_SpellCloudData.Spell);

        #endregion
    }
}