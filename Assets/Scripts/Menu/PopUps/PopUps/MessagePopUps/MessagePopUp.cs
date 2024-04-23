using System;
using TMPro;
using Tools;

namespace Menu.PopUps
{
    public class MessagePopUp : PopUp
    {
        #region Members

        // data
        string m_TitleData;
        string m_Message;

        // GameObjects & Components
        TMP_Text m_MessageText;

        #endregion

        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_MessageText = Finder.FindComponent<TMP_Text>(m_WindowContent, "Message");
        }

        public void Initialize(string message, string title = "")
        {
            base.Initialize();

            m_Message = message;
            m_TitleData = title;
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            m_MessageText.text = m_Message;
            m_Title.text = m_TitleData;
        }

        #endregion

    }
}