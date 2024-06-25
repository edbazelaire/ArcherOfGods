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
    public class CloudDataInterfaceUI : MObject
    {

        #region Members

        CloudData m_CloudDataManager;

        GameObject m_TemplateDataOption;

        TMP_Text m_Title;
        GameObject m_Content;

        string m_Name => m_CloudDataManager != null ? TextHandler.Split(m_CloudDataManager.GetType().ToString().Split(".")[^1]) : "Reset Data";

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_TemplateDataOption = AssetLoader.Load<GameObject>("DataOption", AssetLoader.c_SettingsPath);

            m_Title = Finder.FindComponent<TMP_Text>(gameObject, "Title");
            m_Content = Finder.Find(gameObject, "Content");
        }

        public void Initialize(CloudData cloudDataManager)
        {
            m_CloudDataManager = cloudDataManager;

            base.Initialize();
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            m_Title.text = m_Name;
            SetupDataOptions();
        }

        #endregion


        #region GUI Manipulators

        void SetupDataOptions()
        {
            UIHelper.CleanContent(m_Content);

            DataOptionUI dataOptionUI = Instantiate(m_TemplateDataOption, m_Content.transform).GetComponent<DataOptionUI>();
            dataOptionUI.Initialize("All", m_CloudDataManager);

            if (m_CloudDataManager == null)
                return;

            foreach (string key in m_CloudDataManager.Data.Keys)
            {
                dataOptionUI = Instantiate(m_TemplateDataOption, m_Content.transform).GetComponent<DataOptionUI>();
                dataOptionUI.Initialize(key, m_CloudDataManager);
            }
        }

        #endregion


        #region Listeners


        #endregion
    }
}