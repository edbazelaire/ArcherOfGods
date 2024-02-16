using System;
using Tools;
using UnityEngine;

namespace Menu
{
    public enum EInvetoryItemTab
    {
        Spells,
        Characters,
    }

    public class ItemsTabManager: TabsManager
    {
        protected Type m_TabEnumType { get; set; } = typeof(EInvetoryItemTab);

        GameObject m_TemplateSpellItem; 
        GameObject m_TemplateCharacterItem; 

        public void Initialize()
        {
            m_TemplateSpellItem     = AssetLoader.LoadTemplateItem("SpellItem");
            m_TemplateCharacterItem = AssetLoader.LoadTemplateItem("CharacterButton");
        }
    }
}
