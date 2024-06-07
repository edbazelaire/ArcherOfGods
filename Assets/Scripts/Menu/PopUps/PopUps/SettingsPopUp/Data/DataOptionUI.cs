using Assets;
using Data.GameManagement;
using Save;
using System;
using System.Collections;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class DataOptionUI : MObject
    {

        #region Members

        string m_Key;
        CloudData m_CloudDataManager;

        TMP_Text m_Name;
        Button m_ResetButton;
        Button m_UnlockButton;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Name = Finder.FindComponent<TMP_Text>(gameObject, "Name");
            m_ResetButton = Finder.FindComponent<Button>(gameObject, "ResetButton");
            m_UnlockButton = Finder.FindComponent<Button>(gameObject, "UnlockButton");
        }

        public void Initialize(string key, CloudData cloudDataManager)
        {
            m_Key = key;
            m_CloudDataManager = cloudDataManager;

            base.Initialize();
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            m_Name.text = m_Key;

            if (m_CloudDataManager == null || ! m_CloudDataManager.IsUnlockable(m_Key))
            {
                m_UnlockButton.gameObject.SetActive(false);
            }
        }

        #endregion


        #region GUI Manipulators


        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            m_ResetButton?.onClick.AddListener(OnResetClicked);
            m_UnlockButton?.onClick.AddListener(OnUnlockClicked);
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            m_ResetButton?.onClick.RemoveAllListeners();
            m_UnlockButton?.onClick.RemoveAllListeners();
        }

        void OnResetClicked()
        {
            if (m_CloudDataManager == null)
            {
                Main.CloudSaveManager.ResetAll();
                return;
            }

            if (m_Key == "All")
                m_CloudDataManager.ResetAll();
            else
            {
                m_CloudDataManager.Reset(m_Key);
                m_CloudDataManager.SaveValue(m_Key);
            }
        }

        void OnUnlockClicked()
        {
            m_CloudDataManager.Unlock(m_Key);
        }

        #endregion
    }
}