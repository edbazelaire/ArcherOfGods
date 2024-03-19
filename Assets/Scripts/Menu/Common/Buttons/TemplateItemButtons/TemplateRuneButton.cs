using Enums;
using Game.Managers;
using Inventory;
using Menu.MainMenu;
using Save;
using System;
using Tools;
using UnityEditor;
using UnityEngine;
using static Unity.Collections.Unicode;

namespace Menu.Common.Buttons
{
    public class TemplateRuneButton : TemplateItemButton
    {
        #region Members
        public enum EButtonState
        {
            Locked,
            Normal,
            Updatable
        }

        /// <summary> event that the button has been clicked </summary>
        public static Action<ERune> ButtonClickedEvent;

        // ========================================================================================
        // Button Data
        protected EButtonState          m_State;
        protected ERune                 m_Rune;

        #endregion


        #region Init & End

        public virtual void Initialize(ERune rune)
        {
            base.Initialize();

            // setup UI
            RefreshRune(rune);
        }

        #endregion


        #region GUI Manipulators

        /// <summary>
        /// Setup icon of the spell and color of its border
        /// </summary>
        public virtual void RefreshRune(ERune rune)
        {
            // setup spell data
            m_Rune = rune;
            m_Icon.sprite = AssetLoader.LoadRuneIcon(m_Rune);
        }

        #endregion


        #region State Management

        /// <summary>
        /// Set UI according to the provided state
        /// </summary>
        /// <param name="state"></param>
        protected virtual void SetState(EButtonState state)
        {
            m_State = state;

            switch (state)
            {
                case (EButtonState.Locked):
                    m_LockState.SetActive(true);
                    break;

                case (EButtonState.Updatable):
                case (EButtonState.Normal):
                    m_LockState.SetActive(false);
                    break;

                default:
                    ErrorHandler.Error("UnHandled state " + state);
                    break;
            }
        }

        #endregion


        #region Listeners

        protected override void OnClick()
        {
            base.OnClick();

            ButtonClickedEvent?.Invoke(m_Rune);
        }

        #endregion
    }
}