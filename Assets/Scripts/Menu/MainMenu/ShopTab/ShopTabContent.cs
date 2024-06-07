using Enums;
using Menu.MainMenu.ShopTab;
using Tools;
using UnityEngine;


namespace Menu.MainMenu
{
    public class ShopTabContent : MainMenuTabContent
    {
        #region Members

        /// <summary> handles the preview of the character (Prefab, name, level, ...) </summary>
        SpecialOfferUI              m_SpecialOfferUI;
        /// <summary> handles the Tabs of the inventory (spells, characters, ...) </summary>
        ShopTabManager              m_ShopTabManager;

        #endregion


        #region Init & End

        public override void Initialize(TabButton tabButton, AudioClip activationSoundFX)
        {
            base.Initialize(tabButton, activationSoundFX);

            m_SpecialOfferUI = Finder.FindComponent<SpecialOfferUI>(gameObject, "SpecialOfferSection");
            m_ShopTabManager = Finder.FindComponent<ShopTabManager>(gameObject, "ShopTabManager");

            // initialization
            m_SpecialOfferUI.Initialize();
            m_ShopTabManager.Initialize();
        }

        #endregion
    }
}
