

using Save;
using Tools;
using Tools.Debugs.BetaTest;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class SettingsPopUp : PopUp
    {

        #region Members

        GameObject m_TabButtonContainer;
        SettingsTabManager m_SettingsTabManager;
        GameObject m_WarningMessage;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_TabButtonContainer = Finder.Find(gameObject, "TabButtonsContainer");
            m_SettingsTabManager = Finder.FindComponent<SettingsTabManager>(gameObject);
            m_WarningMessage = Finder.Find(gameObject, "WarningMessage");
        }


        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            if (! ProfileCloudData.IsAdmin) 
            {
                m_WarningMessage.SetActive(false);
                m_TabButtonContainer.SetActive(false);
            } else
            {
                SetUpWarningMessage();
            }

            m_SettingsTabManager.Initialize();
        }

        #endregion


        #region WARNING Message

        void SetUpWarningMessage()
        {
            if (PlayerPrefsHandler.GetWarningAccepted())
            {
                m_WarningMessage.SetActive(false);
                return;
            }

            m_WarningMessage.SetActive(true);
            var acceptButton = Finder.FindComponent<Button>(m_WarningMessage, "Accept");
            var exitButton = Finder.FindComponent<Button>(m_WarningMessage, "Exit");

            acceptButton.onClick.AddListener(OnAcceptWarningMessage);
            exitButton.onClick.AddListener(Exit);
        }

        void OnAcceptWarningMessage()
        {
            PlayerPrefsHandler.SetWarningAccepted(true);
            m_WarningMessage.SetActive(false);
        }

        #endregion
    }
}