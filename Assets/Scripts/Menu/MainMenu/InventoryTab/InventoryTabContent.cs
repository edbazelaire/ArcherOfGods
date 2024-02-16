using Tools;


namespace Menu.MainMenu
{
    public class InventoryTabContent : MainMenuTabContent
    {
        #region Members

        CharacterSectionUI  m_CharacterSectionUI;
        ItemsTabManager     m_ItemsTabManager;
        
        #endregion


        #region Init & End

        public override void Initialize(TabButton tabButton)
        {
            base.Initialize(tabButton);

            m_CharacterSectionUI = Finder.FindComponent<CharacterSectionUI> (gameObject,    "CharacterSectionUI"    );
            m_ItemsTabManager    = Finder.FindComponent<ItemsTabManager>    (gameObject,    "ItemsTabManager"       );
            m_ItemsTabManager.Initialize();
        }


        #endregion
    }
}
