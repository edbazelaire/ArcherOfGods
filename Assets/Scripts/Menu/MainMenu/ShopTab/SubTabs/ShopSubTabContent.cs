using Data.GameManagement;
using Menu.MainMenu.ShopTab;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu.MainMenu
{
    public class ShopSubTabContent : TabContent
    {
        #region Members

        protected ShopScroller        m_Scroller;

        #endregion


        #region Init & End

        public override void Initialize(TabButton tabButton, AudioClip activationSoundFX)
        {
            base.Initialize(tabButton, activationSoundFX);

            m_Scroller = Finder.FindComponent<ShopScroller>(gameObject);

        }

        public override void Activate(bool activate)
        {
            base.Activate(activate);

            gameObject.SetActive(activate);
        }

        #endregion
    }
}