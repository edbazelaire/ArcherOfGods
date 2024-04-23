using System;
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
        protected Image         m_TitleOverlay;
        protected TMP_Text      m_TitleText;
        protected Image         m_BottomOverlay;
        protected TMP_Text      m_BottomText;
        protected GameObject    m_OnSelected;

        // ========================================================================================
        // Button data
        protected bool          m_IsInitialized;
        protected bool          m_AsIconOnly;


        // ========================================================================================
        // Public Accessors
        public Button Button => m_Button;
        public GameObject IconObject => m_Border.gameObject;
        public string Path
        {
            get
            {
                var obj = gameObject;
                string path = "." + obj.name;
                while (obj.transform.parent != null)
                {
                    obj = obj.transform.parent.gameObject;
                    path = "." + obj.name + path;
                }
                return path;
            }
        }

        #endregion


        #region Init & End

        protected virtual void FindComponents()
        {
            // Init GameObjects & Components
            m_Button            = Finder.FindComponent<Button>(gameObject);
            m_LockState         = Finder.Find(gameObject, "LockState", false);
            m_Border            = Finder.FindComponent<Image>(gameObject);
            m_Icon              = Finder.FindComponent<Image>(gameObject, "Icon");
            m_OnSelected        = Finder.Find(gameObject, "OnSelected", false);

            m_TitleOverlay      = Finder.FindComponent<Image>(gameObject, "TitleOverlay", false);
            if (m_TitleOverlay != null)
                m_TitleText         = Finder.FindComponent<TMP_Text>(m_TitleOverlay.gameObject, "Title", false);

            m_BottomOverlay     = Finder.FindComponent<Image>(gameObject, "BottomOverlay", false);
            if (m_BottomOverlay != null)
                m_BottomText        = Finder.FindComponent<TMP_Text>(m_BottomOverlay.gameObject, "BottomText", false);

            // deactivate title by default
            if (m_TitleOverlay != null)
                m_TitleOverlay.gameObject.SetActive(false);

            // deactivate lock state by default
            if (m_LockState != null)
                m_LockState.gameObject.SetActive(false);

            // deactivate "OnSelected" background and/or particles
            SetSelected(false);
        }

        public virtual void Initialize()
        {
            // find game object's components
            FindComponents();

            // register events listeners
            RegisterListeners();

            // signal end of initialization
            m_IsInitialized = true;
        }

        /// <summary>
        /// On destroy : remove all listeners
        /// </summary>
        protected virtual void OnDestroy() 
        {
            UnregisterLiteners();
        }


        protected virtual void RegisterListeners()
        {
            if (m_IsInitialized)
                UnregisterLiteners();

            // Listeners
            m_Button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// Unregister all listeners
        /// </summary>
        protected virtual void UnregisterLiteners()
        {
            if (m_Button == null)
                return;

            m_Button.onClick.RemoveListener(OnClick);
        }

        #endregion


        #region GUI Manipulators

        protected virtual void SetUpUI() { }

        protected virtual void SetTitle(string title, Color? textColor = null, Color? backgroundColor = null)
        {
            if (m_TitleOverlay == null)
            {
                ErrorHandler.Error("Trying to set title " + title + " for object " + name + " but TitleOverlay was not found");
                return;
            }
            if (m_TitleText == null)
            {
                ErrorHandler.Error("Trying to set title " + title + " for object " + name + " but TitleText was not found");
                return;
            }

            m_TitleOverlay.gameObject.SetActive(true);
            m_TitleText.text = title;

            if (textColor.HasValue)
                m_TitleText.color = textColor.Value;

            if (backgroundColor.HasValue)
                m_TitleOverlay.color = backgroundColor.Value;
        }

        public virtual void SetBottomOverlay(string text, Color? textColor = null, Color? backgroundColor = null)
        {
            if (m_BottomOverlay == null)
            {
                ErrorHandler.Error("Trying to set bottom overlay " + text + " for object " + name + " but BottomOverlay was not found");
                return;
            }

            if (m_BottomText == null)
            {
                ErrorHandler.Error("Trying to set bottom overlay " + text + " for object " + name + " but BottomText was not found");
                return;
            }

            m_BottomOverlay.gameObject.SetActive(true);
            m_BottomText.text = text;

            if (textColor.HasValue)
                m_BottomText.color = textColor.Value;

            if (backgroundColor.HasValue)
                m_BottomOverlay.color = backgroundColor.Value;
        }

        protected virtual void SetBorderColor(Color? color)
        {
            if (color == null)
                return;

            if (m_Border == null)
            {
                ErrorHandler.Error("Trying to set border color with color " + color + " for item " + name + " but Border is null");
                return;
            }

            m_Border.color = color.Value;
        }

        protected virtual void SetIcon(Sprite icon)
        {
            if (m_Icon == null)
            {
                ErrorHandler.Error("Trying to set icon for object " + name + " but Icon GameObject was not found");
                return;
            }

            if (icon == null)
                return;

            m_Icon.sprite = icon;
        } 

        protected virtual void SetSelected(bool selected)
        {
            if (m_OnSelected == null)
                return;
            m_OnSelected.SetActive(selected);
        }

        protected virtual void SetColor(Color color)
        {
            SetBorderColor(color);

            if (m_BottomOverlay != null)
                m_BottomOverlay.color = color;

            if (m_TitleOverlay != null)
                m_TitleOverlay.color = color;
        }

        #endregion


        #region Public Accessors

        public virtual void AsIconOnly(bool activate = true)
        {
            m_Button.interactable = ! activate;
            m_AsIconOnly = activate;
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