﻿using Data;
using Enums;
using Game.Loaders;
using Tools;

namespace Menu.Common.Buttons
{
    public class TemplateSpellButton : TemplateCollectableItemUI
    {
        #region Members

        // ========================================================================================
        // Button Data

        protected bool                  m_IsLinked;
        protected bool                  m_IsAutoTarget;

        // ========================================================================================
        // Public Accessors
        public ESpell Spell => (ESpell)m_CollectableCloudData.GetCollectable();

        #endregion

    
        #region GUI Manipulators

        protected override void SetUpUI(bool asIconOnly = false)
        {
            SpellData spellData = SpellLoader.GetSpellData((ESpell)m_Collectable);
            m_IsLinked = spellData.Linked;
            m_IsAutoTarget = spellData.IsAutoTarget;

            base.SetUpUI(asIconOnly);
        }

        /// <summary>
        /// Setup icon of the spell and color of its border
        /// </summary>
        protected virtual void SetUpSpellIconUI()
        {
            m_Icon.sprite = AssetLoader.LoadSpellIcon(Spell);
            SetBottomOverlay(string.Format(LEVEL_FORMAT, m_CollectableCloudData.Level));
            SetColor(SpellLoader.GetRaretyData(Spell).Color);
        }

        #endregion
    }
}