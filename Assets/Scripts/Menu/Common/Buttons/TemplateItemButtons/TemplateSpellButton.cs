using Enums;
using Game.Managers;
using Inventory;
using Menu.MainMenu;
using Save;
using System;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Menu.Common.Buttons
{
    public class TemplateSpellButton : TemplateItemButton
    {
        #region Members
        public enum EButtonState
        {
            Locked,
            Normal,
            Updatable
        }

        /// <summary> event that the button has been clicked </summary>
        public static Action<ESpell> ButtonClickedEvent;

        // ========================================================================================
        // Button Data
        protected EButtonState          m_State;
        protected SSpellCloudData       m_SpellCloudData;
        protected bool                  IsLinkedSpell;

        // ========================================================================================
        // Public Accessors
        public ESpell Spell => m_SpellCloudData.Spell;

        #endregion


        #region Init & End

        public virtual void Initialize(SSpellCloudData spellCloudData)
        {
            base.Initialize();

            // setup spell data
            m_SpellCloudData    = spellCloudData;
            IsLinkedSpell       = SpellLoader.GetSpellData(spellCloudData.Spell).Linked;

            // setup UI
            SetUpSpellIconUI();
        }

        #endregion


        #region GUI Manipulators

        /// <summary>
        /// Setup icon of the spell and color of its border
        /// </summary>
        protected virtual void SetUpSpellIconUI()
        {
            Color raretyColor = SpellLoader.GetRaretyData(m_SpellCloudData.Spell).Color;

            m_Icon.sprite = AssetLoader.LoadSpellIcon(m_SpellCloudData.Spell);
            m_Border.color = raretyColor;
            m_LevelBackground.color = raretyColor;
            m_LevelValue.text = string.Format(LEVEL_FORMAT, m_SpellCloudData.Level);
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
    }
}