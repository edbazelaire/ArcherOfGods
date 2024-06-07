using Data.GameManagement;
using Menu.MainMenu;
using System;
using Tools;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class SettingsTabContent : TabContent
    {
        #region Members

        GameObject m_TemplateSettingOption;

        GameObject  m_Content; 

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_TemplateSettingOption = AssetLoader.Load<GameObject>("SettingOption", AssetLoader.c_SettingsPath);
            m_Content = gameObject;
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            UIHelper.CleanContent(m_Content);
            foreach (ESettings option in Enum.GetValues(typeof(ESettings)))
            {
                SettingOptionUI settingOptionUI = Instantiate(m_TemplateSettingOption, m_Content.transform).GetComponent<SettingOptionUI>();
                settingOptionUI.Initialize(option);
            }
        }

        #endregion


        #region GUI Manipulators

        public override void Activate(bool activate)
        {
            gameObject.SetActive(activate);
        }

        #endregion


        #region Listeners


        #endregion
    }
}