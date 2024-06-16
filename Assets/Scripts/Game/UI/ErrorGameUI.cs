using System.Collections;
using TMPro;
using Tools;
using UnityEngine;

namespace Game.UI
{
    public class ErrorGameUI : MonoBehaviour
    {
        #region Members

        static ErrorGameUI s_Instance;
        public static ErrorGameUI Instance => s_Instance;

        TMP_Text m_MessageText;
        TMP_Text m_SubMessageText;

        #endregion


        #region Init & End

        // Use this for initialization
        void Start()
        {
            s_Instance = this;

            m_MessageText = Finder.FindComponent<TMP_Text>(gameObject, "Message");
            m_SubMessageText = Finder.FindComponent<TMP_Text>(gameObject, "SubMessage");
        }

        #endregion


        #region GUI Manipulators

        public static void Hide()
        {
            Instance.gameObject.SetActive(false);
        }

        public static void Display(string message)
        {
            Instance.gameObject.SetActive(true);
            Instance.m_MessageText.text = message;
            Instance.m_SubMessageText.gameObject.SetActive(false);
        }

        public static void SetSubMessage(string message)
        {
            if (! Instance.m_SubMessageText.gameObject.activeSelf)
            {
                Instance.m_SubMessageText.gameObject.SetActive(true);
            }

            Instance.m_SubMessageText.text = message;
        }

        #endregion


    }
}