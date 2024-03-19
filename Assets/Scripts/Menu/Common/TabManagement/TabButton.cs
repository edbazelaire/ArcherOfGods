using System;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class TabButton : MonoBehaviour
    {
        #region Members

        [SerializeField] Color m_BackgroundColorActivated;

        public Action TabButtonClickedEvent;

        Button m_Button;
        GameObject m_Border;
        Image m_BackgroundImage;
        Image m_Icon;
        Color m_BaseBackgroundColor;

        bool m_Activated;

        #endregion


        #region Init & End

        protected virtual void FindComponents()
        {
            m_Button            = Finder.FindComponent<Button>(gameObject);
            m_Border            = Finder.Find(gameObject, "Border");
            m_BackgroundImage   = Finder.FindComponent<Image>(gameObject, "BackgroundImage");
            m_Icon              = Finder.FindComponent<Image>(gameObject, "TabIcon");

            m_BaseBackgroundColor = m_BackgroundImage.color;

            m_Button.onClick.AddListener(OnButtonClicked);
        }

        public virtual void Initialize()
        {
            FindComponents();
        }

        public virtual void Activate(bool activate)
        {
            // do nothing on activation beeing the same as current activation
            if (m_Activated == activate)
                return;

            m_BackgroundImage.color         = activate ? m_BackgroundColorActivated : m_BaseBackgroundColor;
            m_Icon.transform.localScale     = activate ? new Vector3(1.2f, 1.2f, 1f) : Vector3.one;
            m_Activated                     = activate;
        }

        protected virtual void OnDestroy()
        {
            m_Button.onClick.RemoveAllListeners();
        }

        #endregion


        #region Listeners

        protected virtual void OnButtonClicked()
        {
            TabButtonClickedEvent?.Invoke();
        }

        #endregion
    }
}