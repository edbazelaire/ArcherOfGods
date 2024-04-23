using Assets;
using Assets.Scripts.Menu.Common.Buttons.TemplateItemButtons;
using Data.GameManagement;
using Enums;
using Game.UI;
using Inventory;
using Menu.Common.Buttons;
using Menu.MainMenu;
using Menu.MainMenu.MainTab;
using System;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class StageDisplayUI : MObject
    {
        #region Members

        SArenaLevelData m_ArenaLevelData;

        StageSectionUI  m_StageSectionUI;
        GameObject      m_SpellsContainer;
        GameObject      m_RewardsContainer;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_StageSectionUI = Finder.FindComponent<StageSectionUI>(gameObject, "StageSection");
            m_SpellsContainer = Finder.Find(gameObject, "SpellsContainer");
            m_RewardsContainer = Finder.Find(gameObject, "RewardsContainer");
        }

        public void Initialize(ArenaData arenaData, int arenaLevel)
        {
            base.Initialize();

            m_ArenaLevelData = arenaData.GetArenaLevelData(arenaLevel);

            m_StageSectionUI.Initialize(arenaData, arenaLevel);
            SetUpSpells();
            SetUpRewards();
        }

        #endregion


        #region GUI Manipulators

        void SetUpSpells()
        {
            UIHelper.CleanContent(m_SpellsContainer);
            foreach (ESpell spell in m_ArenaLevelData.Spells)
            {
                TemplateSpellItemUI spellItemUI = Instantiate(AssetLoader.LoadTemplateItem(spell), m_SpellsContainer.transform).GetComponent<TemplateSpellItemUI>();
                spellItemUI.Initialize(spell, asIconOnly: true);

                // TODO : later
                int spellLevel = m_ArenaLevelData.StageData[0].CharacterLevel;
                spellItemUI.SetBottomOverlay("Level " + spellLevel);

                // display informations of the spell on click
                spellItemUI.Button.interactable = true;
                spellItemUI.Button.onClick.RemoveAllListeners();
                spellItemUI.Button.onClick.AddListener(() => { Main.SetPopUp(EPopUpState.SpellInfoPopUp, spell, spellLevel, true); });

            }
        }

        void SetUpRewards()
        {
            UIHelper.CleanContent(m_RewardsContainer);
            var rewards = m_ArenaLevelData.rewardsData.Rewards;
            foreach (SReward reward in rewards)
            {
                // Display CHEST
                if (reward.RewardType == typeof(EChest))
                {
                    var icon = Instantiate(AssetLoader.LoadTemplateItem("Icon"), m_RewardsContainer.transform);
                    icon.GetComponent<Image>().sprite = AssetLoader.LoadIcon(reward.RewardName, reward.RewardType);
                }

                // Display CURRENCY
                else if (reward.RewardType == typeof(ECurrency)) 
                {
                    if (! Enum.TryParse(reward.RewardName, out ECurrency currency))
                    {
                        ErrorHandler.Error("Unable to parse " + reward.RewardName + " into a currency");
                        continue;
                    }

                    var template = Instantiate(AssetLoader.LoadTemplateItem("CurrencyItem"), m_RewardsContainer.transform);
                    template.GetComponent<TemplateCurrencyItem>().Initialize(currency, reward.Qty);
                }

                else
                {
                    var template = Instantiate(AssetLoader.LoadTemplateItem("Collectable"), m_RewardsContainer.transform).GetComponent<TemplateCollectableItemUI>();
                    template.Initialize(CollectablesManagementData.Cast(reward.RewardName, reward.RewardType), asIconOnly: true);
                    template.SetBottomOverlay("x" + reward.Qty);

                    template.Button.interactable = true;
                    template.Button.onClick.RemoveAllListeners();

                    // link spell to spell info popup
                    if (Enum.TryParse(reward.RewardName, out ESpell spell))
                        template.Button.onClick.AddListener(() => { Main.SetPopUp(EPopUpState.SpellInfoPopUp, spell, 1, true); });
                }
            }
        }

        #endregion
    }
}