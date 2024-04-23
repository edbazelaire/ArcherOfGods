using Enums;
using Inventory;
using Save;
using System.Collections;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu
{
    public class HUD : MObject
    {
        #region Members

        GameObject m_ButtonsContainer;
        Button m_ProfileButton;

        #endregion


        #region Init & End

        private void Awake()
        {
            Initialize();
        }

        protected override void FindComponents()
        {
            base.FindComponents();

            m_ButtonsContainer = Finder.Find(gameObject, "ButtonsContainer");
            m_ProfileButton = Finder.FindComponent<Button>(m_ButtonsContainer, "ProfileButton");
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            m_ProfileButton.onClick.AddListener(OnProfileButtonClicked);
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            m_ProfileButton.onClick.RemoveAllListeners();
        }

        void OnProfileButtonClicked()
        {
            Main.ToggleProfileButton();
        }

        #endregion
    }
}