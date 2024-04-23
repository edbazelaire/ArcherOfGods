﻿using Data;
using Data.GameManagement;
using Enums;
using Game.Loaders;
using Inventory;
using Menu.Common.Buttons;
using Menu.Common.Infos;
using System;
using System.Collections.Generic;
using TMPro;
using Tools;
using Tools.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class CollectableInfoPopUp : PopUp
    {
        #region Members

        const string UPGRADE_ANIMATION_ID = "UpgradeAnimation";

        // =========================================================================================
        // Data
        protected CollectionData                        m_Data;
        protected bool                                  m_InfoOnly;
        protected GameObject                            m_InfoPrefab;
        protected GameObject                            m_TemplateItemUI;
        protected Dictionary<string, SpellInfoRowUI>    m_InfoRows;

        // =========================================================================================
        // GameObjects & Components
        protected Image                                 m_RaretyContainer;
        protected TMP_Text                              m_RaretyText;
        protected TemplateCollectableItemUI             m_CollectableItemUI;
        protected GameObject                            m_PreviewContainer;
        protected GameObject                            m_InfosContent;
        protected Button                                m_UpgradeButton;
        protected TMP_Text                              m_CostText;

        // =========================================================================================
        // Dependent Members
        protected Enum m_Collectable                    => m_Data.Id;
        protected int m_Level                           => m_Data.Level;
        protected virtual bool m_IsMaxedLevel           => InventoryManager.IsMaxLevel(m_Collectable);
        protected virtual bool m_CanUpgrade             => InventoryManager.CanUpgrade(m_Collectable);

        #endregion


        #region Init & End

        public void Initialize(Enum enumValue, int level, bool infoOnly = false)
        {
            base.Initialize();

            m_InfoOnly = infoOnly;

            // load data of the item
            if (enumValue.GetType() == typeof(ECharacter))
                m_Data = CharacterLoader.GetCharacterData((ECharacter)enumValue, level);
                
            else if (enumValue.GetType() == typeof(ESpell))
                m_Data = SpellLoader.GetSpellData((ESpell)enumValue, level);
                            
            else if (enumValue.GetType() == typeof(ERune))
                m_Data = SpellLoader.GetSpellData((ESpell)enumValue, level);
        }

        protected override void FindComponents()
        {
            base.FindComponents();

            m_PreviewContainer      = Finder.Find(m_WindowContent, "PreviewContainer");
            m_InfosContent          = Finder.Find(m_WindowContent, "InfosContent");

            m_UpgradeButton         = Finder.FindComponent<Button>(m_Buttons, "UpgradeSubButton", false);
            if (m_UpgradeButton != null)
                m_CostText              = Finder.FindComponent<TMP_Text>(m_UpgradeButton.gameObject, "CostText");

            m_RaretyContainer = Finder.FindComponent<Image>(m_TitleSection, "RaretyContainer", false);
            if (m_RaretyContainer != null)
                m_RaretyText = Finder.FindComponent<TMP_Text>(m_RaretyContainer.gameObject, "RaretyText", false);

            m_InfoPrefab = AssetLoader.Load<GameObject>("SpellInfoRow", AssetLoader.c_MainUIComponentsInfosPath);

            // load data of the item
            m_TemplateItemUI = AssetLoader.LoadTemplateItem(m_Collectable);
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            SetUpTitle();
            SetUpRarety();
            SetUpPreview();
            SetUpAllInfoRows();
            SetUpButtons();
        }

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            InventoryManager.CollectableUpgradedEvent += OnLevelUp;
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            InventoryManager.CollectableUpgradedEvent -= OnLevelUp;
        }

        protected override void Exit()
        {
            AnimationHandler.EndAnimation(UPGRADE_ANIMATION_ID);

            base.Exit();
        }

        #endregion


        #region UIManipulators

        protected virtual void SetUpTitle()
        {
            m_Title.text = TextLocalizer.SplitCamelCase(TextLocalizer.LocalizeText(m_Collectable.ToString()));
        }

        /// <summary>
        /// Instantiate Rarety box of the spell in TitleSection
        /// </summary>
        protected virtual void SetUpRarety()
        {
            if (m_RaretyContainer == null)
                return;

            var raretyData = SpellLoader.GetRaretyData(m_Data.Rarety);
            m_RaretyContainer.color = raretyData.Color;
            m_RaretyText.text = TextLocalizer.LocalizeText(raretyData.Rarety.ToString());
        }

        /// <summary>
        /// Instantiate preview of the spell
        /// </summary>
        protected virtual void SetUpPreview()
        {
            UIHelper.CleanContent(m_PreviewContainer);
            m_CollectableItemUI = Instantiate(m_TemplateItemUI, m_PreviewContainer.transform).GetComponent<TemplateCollectableItemUI>();
            m_CollectableItemUI.Initialize(m_Collectable);

            // deactivate button
            m_CollectableItemUI.Button.interactable = false;
        }

        /// <summary>
        /// Display infos of the spell
        /// </summary>
        protected virtual void SetUpAllInfoRows()
        {
            // clean previous content
            UIHelper.CleanContent(m_InfosContent);
            m_InfoRows = new();

            // -- get new data if spell is updatable
            Dictionary<string, object> newDataInfos = null;
            if (! m_IsMaxedLevel)
                newDataInfos = m_Data.Clone(m_Level + 1).GetInfos();

            var infos = m_Data.GetInfos();
            foreach (var item in infos)
            {
                SetUpInfoRow(item.Key, item.Value, newDataInfos != null ? newDataInfos[item.Key] : null);
            }
        }

        protected virtual void SetUpInfoRow(string key, object value, object newDataValue = null)
        {
            // spawn a spellRowInfo from prefab and init with spell data
            SpellInfoRowUI spellRowInfo = Instantiate(m_InfoPrefab, m_InfosContent.transform).GetComponent<SpellInfoRowUI>();
            spellRowInfo.Initialize(key, value, newDataValue, CheckIsPercentageValue(key));
            m_InfoRows.Add(key, spellRowInfo);
        }

        /// <summary>
        /// Set cost of the UpgradeButton + update UI to match the context
        /// </summary>
        protected virtual void SetUpButtons()
        {
            if (m_IsMaxedLevel || m_InfoOnly) 
            { 
                m_UpgradeButton.gameObject.SetActive(false);
                return;
            }

            RefreshUpgradeButtonUI();
        }

        /// <summary>
        /// Refresh all spell info rows with new value
        /// </summary>
        protected virtual void RefreshInfoRows()
        {
            // -- get new data if spell is updatable
            Dictionary<string, object> newSpelLDataInfos = null;
            if (! m_IsMaxedLevel)
                newSpelLDataInfos = m_Data.Clone(m_Level + 1).GetInfos();

            foreach (var item in m_Data.GetInfos())
            {
                if (!m_InfoRows.ContainsKey(item.Key))
                    continue;

                // get spell row info
                var spellRowInfo = m_InfoRows[item.Key];
                spellRowInfo.RefreshValue(item.Value, newSpelLDataInfos != null ? newSpelLDataInfos[item.Key] : null);
            }
        }

        /// <summary>
        /// Refresh UI to display if the UpgradeButton can be use or not
        /// </summary>
        protected virtual void RefreshUpgradeButtonUI()
        {
            if (m_UpgradeButton == null)
                return;

            if (m_IsMaxedLevel)
            {
                m_UpgradeButton.gameObject.SetActive(false);
                return;
            }

            m_UpgradeButton.interactable = m_CanUpgrade;
            m_CostText.text = CollectablesManagementData.GetLevelData(m_Collectable, m_Level).RequiredGolds.ToString();
        }

        #endregion


        #region Tools

        protected virtual bool CheckIsPercentageValue(string property)
        {
            return property == EStateEffectProperty.LifeSteal.ToString()
                || property.EndsWith("Perc");
        }

        CollectionData LoadCollectionData(Enum enumValue, int level)
        {
            // load data of the item
            if (enumValue.GetType() == typeof(ECharacter))
                return CharacterLoader.GetCharacterData((ECharacter)enumValue, level);
            
            else if (enumValue.GetType() == typeof(ESpell))
                return SpellLoader.GetSpellData((ESpell)enumValue, level);

            ErrorHandler.Error("Unknown CollectionDataType for enum : " + enumValue);

            Exit();
            return null;
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

                default:
                    base.OnUIButton(bname);
                    return;
            }
        }

        protected virtual void OnUpgrade()
        {
            if (! m_CanUpgrade)
                return;

            InventoryManager.Upgrade(m_Collectable);

            if (AnimationHandler.IsPlaying(UPGRADE_ANIMATION_ID))
                return;

            var pulse = m_CollectableItemUI.IconObject.AddComponent<Pulse>();
            pulse.Initialize(UPGRADE_ANIMATION_ID, 2f, 0.9f, 1.1f, pulseDuration: 0.5f, pauseDuration:0f);

            var particles = m_CollectableItemUI.IconObject.AddComponent<ParticlesAnimation>();
            particles.Initialize(UPGRADE_ANIMATION_ID, 2f, particlesName: "LevelUpAnimation", size: 1f, layer: "Overlay");
        }

        /// <summary>
        /// When the value of this button's linked "SSpellCloudData" changes, reload it and apply changes to the UI
        /// </summary>
        /// <param name="spellCloudData"></param>
        protected virtual void OnLevelUp(Enum collectable, int level)
        {
            if (! collectable.Equals(m_Collectable))
                return;

            // reload data
            m_Data = LoadCollectionData(collectable, level);

            // refresh UI
            RefreshUpgradeButtonUI();
            RefreshInfoRows();
        }

        #endregion
    }
}