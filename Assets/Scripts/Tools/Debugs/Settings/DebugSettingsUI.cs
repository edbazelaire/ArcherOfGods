using Data.GameManagement;
using System;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class DebugSettingsUI : MonoBehaviour
    {
        #region Members

        GameObject m_TemplateSettingOption;

        GameObject  m_Content;
        Button      m_ArrowButton;
        Image       m_ArrowIcon;

        #endregion


        #region Init & End

        void Start()
        {
            m_TemplateSettingOption = AssetLoader.Load<GameObject>("SettingOption", AssetLoader.c_SettingsPath);
            m_Content = Finder.Find(gameObject, "Content");
            m_ArrowButton = Finder.FindComponent<Button>(gameObject, "ArrowButton");
            m_ArrowIcon = Finder.FindComponent<Image>(m_ArrowButton.gameObject, "ArrowIcon");

            m_ArrowButton.onClick.AddListener(OnArrowButtonClicked);

            SetUpUI();

            ToggleDisplay(false);
        }

        #endregion


        #region GUI Manipulators

        void SetUpUI()
        {
            UIHelper.CleanContent(m_Content);
            foreach(ESettings option in Enum.GetValues(typeof(ESettings)))
            {
                SettingOptionUI settingOptionUI = Instantiate(m_TemplateSettingOption, m_Content.transform).GetComponent<SettingOptionUI>();
                settingOptionUI.Initialize(option);
            }

            RefreshArrowIcon();
        }

        void ToggleDisplay(bool? display = null)
        {
            if (display == null)
                display = !m_Content.activeInHierarchy;

            m_Content.SetActive(display.Value);
            RefreshArrowIcon();
        }

        void RefreshArrowIcon()
        {
            m_ArrowIcon.transform.rotation = Quaternion.Euler(0f, 0f, m_Content.activeInHierarchy ? 90f : -90f); ;
        }

        #endregion


        #region Listeners

        void OnArrowButtonClicked() 
        {
            ToggleDisplay();
        }

        #endregion


    }
}