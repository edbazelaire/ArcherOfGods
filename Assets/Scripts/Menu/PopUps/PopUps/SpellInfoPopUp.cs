using Data;
using Enums;
using Game.Managers;
using Game.Spells;
using Inventory;
using Menu.Common.Infos;
using Menu.MainMenu;
using Save;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class SpellInfoPopUp : PopUp
    {
        #region Members

        // =========================================================================================
        // Data
        SpellData                           m_SpellData;
        GameObject                          m_SpellRowInfoPrefab;
        GameObject                          m_TemplateSpellItemUI;
        Dictionary<string, SpellInfoRowUI>  m_SpellInfoRows;

        // =========================================================================================
        // GameObjects & Components
        Image                               m_RaretyContainer;
        TMP_Text                            m_RaretyText;
        GameObject                          m_SpellPreviewContainer;
        GameObject                          m_InfosContent;
        Button                              m_UpgradeButton;
        TMP_Text                            m_CostText;
        StateEffectsInfoRow                 m_StateEffectsInfoRow;

        // =========================================================================================
        // Dependent Members
        ESpell m_Spell => m_SpellData.Spell;
        int m_SpellLevel => m_SpellData.Level;
        bool m_CanUpgrade => InventoryManager.GetSpellData(m_Spell).Qty >= SpellLoader.GetSpellLevelData(m_Spell).RequiredCards && InventoryManager.Golds >= SpellLoader.GetSpellLevelData(m_Spell).RequiredGolds && !m_IsMaxedLevel;
        bool m_IsMaxedLevel => InventoryManager.GetSpellData(m_Spell).Level >= SpellLoader.SpellsManagementData.MaxLevel;
        bool m_IsLinked => SpellLoader.GetSpellData(m_Spell).Linked;


        #endregion


        #region Constructor

        public SpellInfoPopUp() : base(EPopUpState.SpellInfoPopUp) { }

        #endregion


        #region Init & End

        public void Initialize(ESpell spell, int level)
        {
            base.Initialize();

            m_SpellData = SpellLoader.GetSpellData(spell, level);
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            m_RaretyContainer           = Finder.FindComponent<Image>(m_TitleSection, "RaretyContainer");
            m_RaretyText                = Finder.FindComponent<TMP_Text>(m_RaretyContainer.gameObject, "RaretyText");
            m_SpellPreviewContainer     = Finder.Find(m_WindowContent, "SpellPreviewContainer");
            m_StateEffectsInfoRow       = Finder.FindComponent<StateEffectsInfoRow>(gameObject);
            m_InfosContent              = Finder.Find(m_WindowContent, "InfosContent");
            m_UpgradeButton             = Finder.FindComponent<Button>(m_Buttons, "UpgradeSubButton");
            m_CostText                  = Finder.FindComponent<TMP_Text>(m_UpgradeButton.gameObject, "CostText");
            
            m_TemplateSpellItemUI       = AssetLoader.LoadTemplateItem("SpellItem");
            m_SpellRowInfoPrefab        = AssetLoader.Load<GameObject>("SpellInfoRow", AssetLoader.c_MainUIComponentsInfosPath);

            SetUpTitle();
            SetUpSpellRarety();
            SetUpSpellPreview();
            SetUpSpellInfos();
            SetUpButtons();

            InventoryCloudData.SpellDataChangedEvent += OnSpellDataChanged;
        }

        protected override void OnExit()
        {
            base.OnExit();

            InventoryCloudData.SpellDataChangedEvent -= OnSpellDataChanged;
        }

        #endregion


        #region UIManipulators

        void SetUpTitle()
        {
            m_Title.text = TextLocalizer.SplitCamelCase(TextLocalizer.LocalizeText(m_Spell.ToString()));
        }

        /// <summary>
        /// Instantiate Rarety box of the spell in TitleSection
        /// </summary>
        void SetUpSpellRarety()
        {
            var raretyData = SpellLoader.GetRaretyData(m_Spell);
            m_RaretyContainer.color = raretyData.Color;
            m_RaretyText.text = TextLocalizer.LocalizeText(raretyData.Rarety.ToString());
        }

        /// <summary>
        /// Instantiate preview of the spell
        /// </summary>
        void SetUpSpellPreview()
        {
            UIHelper.CleanContent(m_SpellPreviewContainer);
            var spellItemUI = Instantiate(m_TemplateSpellItemUI, m_SpellPreviewContainer.transform).GetComponent<TemplateSpellItemUI>();
            spellItemUI.Initialize(m_Spell);

            // deactivate button
            spellItemUI.Button.interactable = false;
        }

        /// <summary>
        /// Display infos of the spell
        /// </summary>
        void SetUpSpellInfos()
        {
            // clean previous content
            UIHelper.CleanContent(m_InfosContent);
            m_SpellInfoRows = new();

            // get data of the spell
            var spellData = SpellLoader.GetSpellData(m_Spell, m_SpellLevel);
            // -- get new data if spell is updatable
            Dictionary<string, object> newSpelLDataInfos = null;
            if (! m_IsMaxedLevel)
                newSpelLDataInfos = SpellLoader.GetSpellData(m_Spell, m_SpellLevel + 1).GetInfos();

            var spellDataInfos = spellData.GetInfos();
            foreach (var item in spellDataInfos)
            {
                if (item.Key == "Effects")
                    continue;

                // spawn a spellRowInfo from prefab and init with spell data
                SpellInfoRowUI spellRowInfo = Instantiate(m_SpellRowInfoPrefab, m_InfosContent.transform).GetComponent<SpellInfoRowUI>();

                if (item.Key == "Type")
                    spellRowInfo.Initialize(item.Key, item.Value);
                else
                {
                    float? newValue = null;
                    if (newSpelLDataInfos != null && float.TryParse(newSpelLDataInfos[item.Key].ToString(), out float newValueTemp))
                    {
                        newValue = newValueTemp;
                    }

                    if (!float.TryParse(item.Value.ToString(), out float value))
                    {
                        ErrorHandler.Error("Unable to parse " + item.Value + " as float");
                        continue;
                    }

                    spellRowInfo.Initialize(item.Key, value, newValue, CheckIsPercentageValue(item.Key));
                }

                m_SpellInfoRows.Add(item.Key, spellRowInfo);
            }

            if (spellDataInfos.ContainsKey("Effects") && (spellDataInfos["Effects"] as List<SStateEffectData>).Count > 0)
            {
                m_StateEffectsInfoRow.gameObject.SetActive(true);
                m_StateEffectsInfoRow.Initialize(spellDataInfos["Effects"] as List<SStateEffectData>, m_SpellLevel);
            } else
            {
                m_StateEffectsInfoRow.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set cost of the UpgradeButton + update UI to match the context
        /// </summary>
        void SetUpButtons()
        {
            if (m_IsLinked || m_IsMaxedLevel) 
            { 
                m_UpgradeButton.gameObject.SetActive(false);
                return;
            }

            RefreshUpgradeButtonUI();
        }

        /// <summary>
        /// Refresh all spell info rows with new value
        /// </summary>
        void RefreshInfoRows()
        {
            // -- get new data if spell is updatable
            Dictionary<string, object> newSpelLDataInfos = null;
            if (! m_IsMaxedLevel)
                newSpelLDataInfos = SpellLoader.GetSpellData(m_Spell, m_SpellLevel + 1).GetInfos();

            foreach (var item in m_SpellData.GetInfos())
            {
                // if value can not be parsed in float : skip
                if (!float.TryParse(item.Value.ToString(), out float value))
                    continue;

                // get spell row info
                var spellRowInfo = m_SpellInfoRows[item.Key];

                // check if a next value can be casted in float
                float? newValue = null;
                if (newSpelLDataInfos != null && float.TryParse(newSpelLDataInfos[item.Key].ToString(), out float newValueTemp))
                {
                    newValue = newValueTemp;
                }

                spellRowInfo.RefreshValue(value, newValue);
            }
        }

        /// <summary>
        /// Refresh UI to display if the UpgradeButton can be use or not
        /// </summary>
        void RefreshUpgradeButtonUI()
        {
            var buttonImage = Finder.FindComponent<Image>(m_UpgradeButton.gameObject);

            m_UpgradeButton.interactable = m_CanUpgrade;
            buttonImage.color = m_CanUpgrade ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            m_CostText.text = SpellLoader.GetSpellLevelData(m_Spell).RequiredGolds.ToString();
        }

        #endregion


        #region Tools

        bool CheckIsPercentageValue(string property)
        {
            return property == EStateEffectProperty.LifeSteal.ToString()
                || property.EndsWith("Perc");
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

        void OnUpgrade()
        {
            InventoryManager.Upgrade(InventoryCloudData.Instance.GetSpell(m_Spell));
        }

        /// <summary>
        /// When the value of this button's linked "SSpellCloudData" changes, reload it and apply changes to the UI
        /// </summary>
        /// <param name="spellCloudData"></param>
        void OnSpellDataChanged(SSpellCloudData spellCloudData)
        {
            m_SpellData = SpellLoader.GetSpellData(m_Spell, InventoryManager.GetSpellData(m_Spell).Level);

            RefreshUpgradeButtonUI();
            RefreshInfoRows();
            m_StateEffectsInfoRow.RefreshValue(m_SpellLevel);
        }

        #endregion
    }
}