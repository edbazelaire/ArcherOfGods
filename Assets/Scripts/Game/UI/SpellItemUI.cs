using Data;
using Enums;
using Game.Managers;
using TMPro;
using Tools;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SpellItemUI : MonoBehaviour
    {
        #region Members

        /// <summary> name of the GameObject containing the IconImage </summary>
        const string    c_IconImage     = "IconImage";
        /// <summary> name of the GameObject containing the border </summary>
        const string    c_Border        = "Border";
        /// <summary> name of the GameObject containing the cooldown </summary>
        const string    c_Cooldown      = "Cooldown";
        /// <summary> name of the GameObject containing the cooldown counter </summary>
        const string    c_CooldownCtr   = "CooldownCtr";

        Controller m_Owner;

        /// <summary> Image of the spell icon </summary>
        Image           m_IconImage;
        /// <summary> Image of the border </summary>
        Image           m_Border;
        /// <summary> Button of the spell </summary>
        Button          m_Button;
        /// <summary> GameObject of the cooldown containing the UI of the Cooldowns </summary>
        GameObject      m_Cooldown;
        /// <summary> TextMeshPro of the cooldown counter </summary>
        TMP_Text        m_CooldownCtr;
        /// <summary> Spell to which the SpellItemUI is linked </summary>
        ESpell         m_Spell;
        /// <summary> Is the cooldown activated ? </summary>
        bool           m_IsCooldownActivated;

        float m_BaseCooldown;
        float m_CooldownValue;

        public ESpell Spell => m_Spell;

        #endregion


        #region Inherited Manipulators

        private void Update()
        {
            UpdateCooldown();            
        }

        #endregion


        #region Initialization
        public void Initialize(ESpell spell)
        {
            m_Owner = GameManager.Instance.Owner;
            m_Spell = spell;

            SpellData spellData = SpellLoader.GetSpellData(m_Spell);
            m_BaseCooldown = spellData.Cooldown;
            m_CooldownValue = 0;

            SetupIcon();
            SetupButton();

            // register to spell selection changes to update the border if needed
            m_Owner.SpellHandler.SelectedSpellNet.OnValueChanged += OnSpellSelected;
            m_Owner.SpellHandler.OnSpellCasted += OnSpellCasted;
        }

        /// <summary>
        /// Setup the icon of the spell in the SpellItemContainer
        /// </summary>
        public void SetupIcon()
        {
            m_IconImage     = Finder.FindComponent<Image>(gameObject, c_IconImage);
            m_Border        = Finder.FindComponent<Image>(gameObject, c_Border);
            m_Cooldown      = Finder.Find(gameObject, c_Cooldown);
            m_CooldownCtr   = Finder.FindComponent<TMP_Text>(m_Cooldown, c_CooldownCtr);

            SpellData spellData = SpellLoader.GetSpellData(m_Spell);
            m_IconImage.sprite = spellData.Image;

            // deactivate cooldown by default
            m_Cooldown.SetActive(false);
        }

        /// <summary>
        /// Get Button and link to OnClick method
        /// </summary>
        public void SetupButton()
        {
            m_Button = Finder.FindComponent<Button>(gameObject);
            m_Button.onClick.AddListener(OnClick);
        }

        #endregion


        #region Private Manipulators   
        
        /// <summary>
        /// Ask for spell selection to the server
        /// </summary>
        void OnClick()
        {
            GameManager.Instance.Owner.SpellHandler.AskSpellSelectionServerRPC(m_Spell);
        }

        /// <summary>
        /// When the spell selection changes, update the border if needed
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnSpellSelected(int oldValue, int newValue)
        {
            SetSpellSelected((ESpell)newValue == m_Spell);
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

            m_CooldownValue = m_BaseCooldown;
        }

        /// <summary>
        /// When the spell selection changes, update the border if needed
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public void SetSpellSelected(bool selected)
        {
            m_Border.color = selected ? Color.red : Color.black;
        }

        /// <summary>
        /// When the cooldown changes, update the cooldown if needed
        /// </summary>
        /// <param name="changeEvent"></param>
        void UpdateCooldown()
        {
            if (m_CooldownValue <= 0)
                return;

            m_CooldownValue -= Time.deltaTime; 
            if (m_CooldownValue <= 0)
            {
                if (!m_IsCooldownActivated)
                    return;

                m_CooldownValue = 0;

                // todo : play end cooldown animation
                m_IsCooldownActivated = false;
                m_Cooldown.SetActive(false);
            }

            else if (!m_IsCooldownActivated)
            {
                // play cooldown animation (?)
                m_IsCooldownActivated = true;
                m_Cooldown.SetActive(true);
            }
            
            m_CooldownCtr.text = m_CooldownValue.ToString("0");
        }

        #endregion
    }
}