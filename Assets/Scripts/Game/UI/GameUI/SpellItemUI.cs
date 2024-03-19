using Data;
using Enums;
using Game.Managers;
using Menu.Common.Buttons;
using Save;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SpellItemUI : TemplateSpellButton
    {
        #region Members

        /// <summary> name of the GameObject containing the cooldown counter </summary>
        const string    c_CooldownCtr   = "CooldownCtr";

        /// <summary> Owner of this spell item (= current player) </summary>
        Controller      m_Owner;

        /// <summary> TextMeshPro of the cooldown counter </summary>
        TMP_Text        m_CooldownCtr;

        /// <summary> base cooldown of the spell </summary>
        float m_BaseCooldown;
        /// <summary> client side cooldown that handles spell cooldown display (to avoid spamming server and delays) </summary>
        float m_CooldownTimer;

        ESpell m_Spell => m_SpellCloudData.Spell;

        #endregion


        #region Inherited Manipulators

        private void Update()
        {
            // not initialized yet : skip update
            if (!m_IsInitialized)
                return;

            // game over : stop updating
            if (GameManager.Instance.IsGameOver)
                return;

            // not locked -> skip update
            if (m_State != EButtonState.Locked)
                return;

            UpdateState();
            UpdateCooldown();            
        }

        #endregion


        #region Initialization
        /// <summary>
        /// Initialize the GameObject : graphics, button, members, listeners
        /// </summary>
        /// <param name="spell"></param>
        public void Initialize(ESpell spell, int level)
        {
            base.Initialize(new SSpellCloudData(spell, level, 0));

            m_Owner = GameManager.Instance.Owner;

            SpellData spellData = SpellLoader.GetSpellData(m_Spell, level);
            m_BaseCooldown  = spellData.Cooldown;
            m_CooldownTimer = 0;

            SetupCooldown();
            SetupButton();

            // set initial state
            SetState(m_Owner.SpellHandler.CanSelect(m_Spell) ? EButtonState.Normal : EButtonState.Locked);

            // listeners
            m_Owner.SpellHandler.SelectedSpellNet.OnValueChanged += OnSpellSelected;
            m_Owner.SpellHandler.OnSpellCasted += OnSpellCasted;
        }

        /// <summary>
        /// Setup the icon of the spell in the SpellItemContainer
        /// </summary>
        public void SetupCooldown()
        {
            m_CooldownCtr   = Finder.FindComponent<TMP_Text>(m_LockState, c_CooldownCtr);
        }

        /// <summary>
        /// Get Button and link to OnClick method
        /// </summary>
        public void SetupButton()
        {
            m_Button        = Finder.FindComponent<Button>(gameObject);
        }

        #endregion


        #region Private Manipulators 
        
        void UpdateState()
        {
            if (m_Owner.SpellHandler.CanSelect(m_Spell))
            {
                SetState(EButtonState.Normal);
                return;
            }

            // no cooldown : hide the counter
            if (m_CooldownTimer <= 0)
                m_CooldownCtr.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// When the cooldown changes, update the cooldown if needed
        /// </summary>
        /// <param name="changeEvent"></param>
        void UpdateCooldown()
        {
            if (m_CooldownTimer <= 0)
                return;

            m_CooldownTimer -= Time.deltaTime; 
            if (m_CooldownTimer <= 0)
            {
                if (m_State != EButtonState.Locked)
                    return;

                m_CooldownTimer = 0;

                // todo : play end cooldown animation
                SetState(EButtonState.Normal);
            }

            m_CooldownCtr.text = m_CooldownTimer.ToString("0");
        }

        #endregion


        #region Listeners

        /// <summary>
        /// Ask for spell selection to the server
        /// </summary>
        protected override void OnClick()
        {
            if (! m_Owner.SpellHandler.CanSelect(m_Spell))
                // TODO : CantSelectSpellFeedback()
                return;

            SetSelected(true);
            m_Owner.SpellHandler.AskSpellSelectionServerRPC(m_Spell);
        }

        /// <summary>
        /// When the spell selection changes, update the border if needed
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnSpellSelected(int oldValue, int newValue)
        {
            SetSelected((ESpell)newValue == m_Spell);
        }

        /// <summary>
        /// When the spell selection changes, update the border if needed
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnSpellCasted(ESpell spell)
        {
            if (spell != m_Spell)
                return;

            m_CooldownTimer = m_BaseCooldown;
            m_CooldownCtr.gameObject.SetActive(true);
            SetState(EButtonState.Locked);
        }

        #endregion
    }
}