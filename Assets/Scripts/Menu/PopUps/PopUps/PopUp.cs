using Enums;
using System;
using TMPro;
using Tools;
using Tools.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class PopUp : OverlayScreen
    {
        #region Members

        // ==========================================================================================
        // GameObjects & Components
        protected GameObject    m_PopUpWindow;
        protected Image         m_Background;
        // -- title section
        protected GameObject    m_TitleSection;
        protected TMP_Text      m_Title;
        // -- window content
        protected GameObject    m_WindowContent;
        // -- button section
        protected GameObject    m_Buttons;

        // ==========================================================================================
        // Data (NOT IMPLEMENTED YET : add validation callbacks from external sources than the popup)
        protected Action        m_OnValidate    = null;
        protected Action        m_OnCancel       = null;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_PopUpWindow       = Finder.Find(gameObject, "PopUpWindow");
            m_Background        = Finder.FindComponent<Image>(gameObject, "Background");
            m_TitleSection      = Finder.Find(gameObject, "TitleSection", false);
            m_Title             = Finder.FindComponent<TMP_Text>(gameObject, "Title", false);
            m_WindowContent     = Finder.Find(gameObject, "WindowContent");
            m_Buttons           = Finder.Find(gameObject, "Buttons", false);
        }

        protected override void OnInitializationCompleted()
        {
            base.OnInitializationCompleted();

            var fadeIn = m_PopUpWindow.AddComponent<Fade>();
            fadeIn.Initialize("", duration:0.2f, startScale:0);
        }

        protected override void OnExit()
        {
            base.OnExit();

            // delay destruction of the game object
            m_EndingTimer = 0.2f;

            // set fade animation
            var fadeOut = m_PopUpWindow.AddComponent<Fade>();
            fadeOut.Initialize("", duration: m_EndingTimer, endScale: 0);
        }

        #endregion


        #region Inherited Manipulators

        protected override void OnUIButton(string bname)
        {
            switch (bname)
            {
                case "Background":
                    OnCancelButton();
                    break;

                case "ValidateButton":
                    OnValidateButton();
                    break;

                default:
                    base.OnUIButton(bname);
                    break;
            }
        }

        #endregion


        #region Listeners

        protected override void OnCancelButton()
        {
            m_OnCancel?.Invoke();
            base.OnCancelButton();
        }

        protected void OnValidateButton()
        {
            m_OnValidate?.Invoke();
        }

        #endregion
    }
}