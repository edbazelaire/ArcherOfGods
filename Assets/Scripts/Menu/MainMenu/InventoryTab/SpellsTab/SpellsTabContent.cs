using Enums;
using Game.Managers;
using Inventory;
using Save;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace Menu.MainMenu
{
    public class SpellsTabContent : TabContent
    {
        #region Members

        const string SPELL_ITEM_NAME_FORMAT = "SpellItem_{0}";

        GameObject m_TemplateSpellItem;
        Dictionary<ESpell, TemplateSpellItemUI> m_SpellItems = new();

        GameObject m_SpellItemContainer;
        GameObject m_LockedSpellItemContainer;

        #endregion


        #region Init & End

        void Awake()
        {
            m_TemplateSpellItem             = AssetLoader.LoadTemplateItem("SpellItem");
            m_SpellItemContainer            = Finder.Find(gameObject, "SpellItemContainer");
            m_LockedSpellItemContainer      = Finder.Find(gameObject, "LockedSpellItemContainer");

            // Listeners
            CharacterBuildsCloudData.SelectedCharacterChangedEvent += RefreshSpellItemsDisplay;
            CharacterBuildsCloudData.CurrentBuildIndexChangedEvent += RefreshSpellItemsDisplay;
            CharacterBuildsCloudData.CurrentBuildValueChangedEvent += RefreshSpellItemsDisplay; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabButton"></param>
        public override void Initialize(TabButton tabButton)
        {
            base.Initialize(tabButton);

            // remove content in spell items displayers
            UIHelper.CleanContent(m_SpellItemContainer);
            UIHelper.CleanContent(m_LockedSpellItemContainer);

            m_SpellItems = new Dictionary<ESpell, TemplateSpellItemUI>();
            foreach (ESpell spell in SpellLoader.Spells)
            {
                // skip if spell is linked to a character
                if (SpellLoader.GetSpellData(spell).Linked)
                    continue;

                // check if is unlocked or not
                var parent = InventoryCloudData.Instance.GetSpell(spell).Level > 0 ? m_SpellItemContainer.transform : m_LockedSpellItemContainer.transform;

                // spawn and init ui of the spell
                TemplateSpellItemUI spellUI = Instantiate(m_TemplateSpellItem, parent).GetComponent<TemplateSpellItemUI>();
                spellUI.gameObject.name = string.Format(SPELL_ITEM_NAME_FORMAT, spell.ToString());
                spellUI.Initialize(spell);

                if (InventoryCloudData.Instance.GetSpell(spell).Level > 0)
                    m_SpellItems.Add(spell, spellUI);

                // hide if spell is in current build
                if (CharacterBuildsCloudData.CurrentBuild.Contains(spell))
                    spellUI.gameObject.SetActive(false);
            }

            InventoryManager.UnlockSpellEvent += OnUnlockedSpell;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activate"></param>
        public override void Activate(bool activate)
        {
            base.Activate(activate);

            gameObject.SetActive(activate);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            CharacterBuildsCloudData.SelectedCharacterChangedEvent -= RefreshSpellItemsDisplay;
            CharacterBuildsCloudData.CurrentBuildIndexChangedEvent -= RefreshSpellItemsDisplay;
            CharacterBuildsCloudData.CurrentBuildValueChangedEvent -= RefreshSpellItemsDisplay;
        }

        #endregion


        #region GUI Manipulators

        /// <summary>
        /// When a build or character selected is changed, refresh which spell item is displayed or not
        /// </summary>
        public void RefreshSpellItemsDisplay()
        {
            foreach (var item in m_SpellItems)
            {
                // set spell active only if not in current build
                item.Value.gameObject.SetActive( ! CharacterBuildsCloudData.CurrentBuild.Contains(item.Key) );
            }
        }
        
        /// <summary>
        /// Show or hide a spell item
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="show"></param>
        public static void ShowSpellItem(ESpell spell, bool show)
        {
            var spellItems = Resources.FindObjectsOfTypeAll(typeof(TemplateSpellItemUI));
            foreach (var spellItem in spellItems)
            {
                if (spellItem.name == string.Format(SPELL_ITEM_NAME_FORMAT, spell.ToString()))
                {
                    spellItem.GameObject().SetActive(show);
                    return;
                }
            }

            ErrorHandler.Error("Unable to find spell item : " + string.Format(SPELL_ITEM_NAME_FORMAT, spell.ToString()));
        }

        #endregion


        #region Listeners

        /// <summary>
        /// When a spell is unlock, change its parent from Locked to normal SpellItemContainer
        /// </summary>
        /// <param name="spell"></param>
        void OnUnlockedSpell(ESpell spell)
        {
            // find the item
            TemplateSpellItemUI spellItemUI = Finder.FindComponent<TemplateSpellItemUI>(m_LockedSpellItemContainer, string.Format(SPELL_ITEM_NAME_FORMAT, spell.ToString()));

            // change parent
            spellItemUI.transform.parent = m_SpellItemContainer.transform;

            // add spellUI to dict of spell UIs
            m_SpellItems.Add(spellItemUI.Spell, spellItemUI);
        }

        #endregion
    }
}