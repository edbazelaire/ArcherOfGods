using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu.MainMenu
{
    public class CharactersTabContent : TabContent
    {
        /// <summary> handles the display of the character template buttons </summary>
        CharacterSelectionUI m_CharacterSelectionUI;

        #region Init & End

        public override void Initialize(TabButton tabButton)
        {
            base.Initialize(tabButton);

            m_CharacterSelectionUI = Finder.FindComponent<CharacterSelectionUI>(gameObject);
            m_CharacterSelectionUI.Initialize();
        }

        public override void Activate(bool activate)
        {
            base.Activate(activate);

            gameObject.SetActive(activate);
        }

        #endregion
    }
}