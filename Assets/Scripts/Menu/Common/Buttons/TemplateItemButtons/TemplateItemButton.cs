using Enums;
using Save;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Common.Buttons
{
    public class TemplateItemButton : MonoBehaviour
    {

        #region Members

        // ========================================================================================
        // Constants
        protected const string LEVEL_FORMAT = "Level {0}";

        // ========================================================================================
        // GameObjects & Components
        protected Button        m_Button;

        // -- icon & border
        protected GameObject    m_LockState;
        protected Image         m_Icon;
        protected Image         m_Border;
        protected Image         m_LevelBackground;
        protected TMP_Text      m_LevelValue;
        protected GameObject    m_OnSelected;

        // ========================================================================================
        // Button data
        protected bool          m_IsInitialized;

        // ========================================================================================
        // Public Accessors
        public Button Button => m_Button;

        #endregion


        #region Init & End

        protected virtual void FindComponents()
        {
            // Init GameObjects & Components
            m_Button            = Finder.FindComponent<Button>(gameObject);
            m_LockState         = Finder.Find(gameObject, "LockState", false);
            m_Border            = Finder.FindComponent<Image>(gameObject);
            m_Icon              = Finder.FindComponent<Image>(gameObject, "Icon");
            m_LevelBackground   = Finder.FindComponent<Image>(gameObject, "LevelBackground", false);
            m_LevelValue        = Finder.FindComponent<TMP_Text>(gameObject, "LevelValue", false);
            m_OnSelected        = Finder.Find(gameObject, "OnSelected", false);

            // deactivate "OnSelected" background and/or particles
            SetSelected(false);

            // Listeners
            m_Button.onClick.AddListener(OnClick);
        }

        public virtual void Initialize()
        {
            // find game object's components
            FindComponents();

            // signal end of initialization
            m_IsInitialized = true;
        }

        /// <summary>
        /// On destroy : remove all listeners
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_Button == null)
                return;

            m_Button.onClick.RemoveAllListeners();
        }

        #endregion


        #region GUI Manipulators

        protected virtual void SetSelected(bool selected)
        {
            if (m_OnSelected == null)
                return;
            m_OnSelected.SetActive(selected);
        }

        #endregion


        #region Public Accessors

        public virtual void AsIconOnly(bool activate = true)
        {
            m_Button.interactable = ! activate;
        }

        #endregion


        #region Listeners

        /// <summary>
        /// Action happening when the button is clicked on - depending on button context
        /// </summary>
        protected virtual void OnClick() { }

        #endregion
    }
}