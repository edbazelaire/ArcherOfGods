﻿using System;
using TMPro;
using Tools;
using UnityEngine;

namespace Menu.PopUps
{
    public class MessagePopUp : PopUp
    {
        #region Members

        // data
        string m_TitleData;
        string m_Message;

        // GameObjects & Components
        GameObject m_MessageContainer;
        TMP_Text m_MessageText;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_MessageContainer = Finder.Find(m_WindowContent, "MessageContainer");
            m_MessageText = Finder.FindComponent<TMP_Text>(m_WindowContent, "Message");
        }

        public void Initialize(string message, string title = "", Action onValidate = null, Action onCancel = null)
        {
            base.Initialize(onValidate, onCancel);

            m_Message = message;
            m_TitleData = title;
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            SetUpTitle();
            SetUpMessage();
            
        }

        #endregion


        #region GUI Manipulators

        protected virtual void SetUpTitle()
        {
            if (m_Title == null)
                return;
            
            if (m_TitleData == null || m_TitleData == "")
            {
                m_Title.gameObject.SetActive(false);
            }

            m_Title.text = m_TitleData;
        }


        protected virtual void SetUpMessage()
        {
            if (m_Message == null || m_Message == "")
            {
                if (m_MessageContainer != null)
                    m_MessageContainer.SetActive(false);
                else if (m_Message != null)
                    m_MessageText.gameObject.SetActive(false);

                return;
            }

            if (m_MessageText == null)
                return;

            m_MessageText.text = m_Message;
        }

        #endregion

    }
}