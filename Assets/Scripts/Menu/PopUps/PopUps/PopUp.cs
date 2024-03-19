using Enums;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class PopUp : OverlayScreen
    {
        #region Members

        protected override string PrefabPath { get; set; } = AssetLoader.c_PopUpsPath;

        // ==========================================================================================
        // GameObjects & Components
        protected GameObject    m_PopUpWindow;
        // -- title section
        protected GameObject    m_TitleSection;
        protected TMP_Text      m_Title;
        // -- window content
        protected GameObject    m_WindowContent;
        // -- button section
        protected GameObject    m_Buttons;

        #endregion


        #region Constructor 

        public PopUp(EPopUpState state) : base(state) { }

        #endregion


        #region Init & End

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            m_PopUpWindow       = Finder.Find(gameObject, "PopUpWindow");
            m_TitleSection      = Finder.Find(gameObject, "TitleSection", false);
            m_Title             = Finder.FindComponent<TMP_Text>(gameObject, "Title", false);
            m_WindowContent     = Finder.Find(gameObject, "WindowContent");
            m_Buttons           = Finder.Find(gameObject, "Buttons", false);
        }

        #endregion


        #region Inherited Manipulators

        protected override void OnUIButton(string bname)
        {
            switch (bname)
            {
                case "Background":
                    OnCancel();
                    break;

                case "CancelButton":
                    OnCancel();
                    break;

                default:
                    base.OnUIButton(bname);
                    break;
            }
        }

        #endregion


        #region Listeners

        void OnCancel()
        {
            OnExit();
        }

        #endregion
    }
}