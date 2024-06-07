using Data.GameManagement;
using Enums;
using Menu.MainMenu;
using System;
using Tools;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class SoundTabContent : TabContent
    {
        #region Members

        GameObject m_TemplateSettingOption;

        GameObject  m_Content; 

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_TemplateSettingOption = AssetLoader.Load<GameObject>("VolumeOption", AssetLoader.c_SettingsPath);
            m_Content = Finder.Find(gameObject, "Content");
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            UIHelper.CleanContent(m_Content);
            foreach (EVolumeOption option in Enum.GetValues(typeof(EVolumeOption)))
            {
                VolumeOptionUI settingOptionUI = Instantiate(m_TemplateSettingOption, m_Content.transform).GetComponent<VolumeOptionUI>();
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