﻿using Enums;
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
        Button m_SettingsButton;
        Button m_ConsoleButton;

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
            m_SettingsButton = Finder.FindComponent<Button>(m_ButtonsContainer, "SettingsButton");
            m_ConsoleButton = Finder.FindComponent<Button>(m_ButtonsContainer, "ConsoleButton");
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            m_SettingsButton.onClick.AddListener(() => Main.SetPopUp(EPopUpState.SettingsPopUp));
            m_ConsoleButton.onClick.AddListener(() => ConsoleUI.Instance.Hide());
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            m_SettingsButton.onClick.RemoveAllListeners();
        }

        #endregion
    }
}