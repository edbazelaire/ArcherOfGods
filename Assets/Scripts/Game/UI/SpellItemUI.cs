using Data;
using Enums;
using Game.Managers;
using TMPro;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SpellItemUI 
    {
        #region Members

        const string    c_IconImage     = "IconImage";
        const string    c_Border        = "Border";
        const string    c_Cooldown      = "Cooldown";
        const string    c_CooldownCtr   = "CooldownCtr";

        GameObject      m_GameObject;
        Image           m_IconImage;
        Image           m_Border;
        Button          m_Button;
        GameObject      m_Cooldown;
        TMP_Text        m_CooldownCtr;
        ESpells         m_Spell;
        bool            m_IsCooldownActivated;

        #endregion


        #region Constructor

        public SpellItemUI(GameObject gameObject, ESpells eSpells)
        {
            m_GameObject        = gameObject;
            m_Spell             = eSpells;

            SetupIcon();
            SetupButton();

            GameManager.Instance.Owner.SpellHandler.SelectedSpellNet.OnValueChanged += OnSpellSelected;
        }

        #endregion


        #region Initialization

        public void SetupIcon()
        {
            m_IconImage     = Finder.FindComponent<Image>(m_GameObject, c_IconImage);
            m_Border        = Finder.FindComponent<Image>(m_GameObject, c_Border);
            m_Cooldown      = Finder.Find(m_GameObject, c_Cooldown);
            m_CooldownCtr   = Finder.FindComponent<TMP_Text>(m_Cooldown, c_CooldownCtr);

            SpellData spellData = SpellLoader.GetSpellData(m_Spell);
            m_IconImage.sprite = spellData.Image;

            m_Cooldown.SetActive(false);
        }

        public void SetupButton()
        {
            m_Button = m_GameObject.GetComponent<Button>();
            m_Button.onClick.AddListener(OnClick);
        }

        #endregion


        #region Private Manipulators   
        
        void OnClick()
        {
            GameManager.Instance.Owner.SpellHandler.AskSpellSelection(m_Spell);
        }

        void OnSpellSelected(int oldValue, int newValue)
        {
            m_Border.color = (ESpells)newValue == m_Spell ? Color.red : Color.black;
        }

        void UpdateCooldown()
        {
            float cooldown = GameManager.Instance.Owner.SpellHandler.GetCooldown(m_Spell);
            if (cooldown <= 0)
            {
                if (!m_IsCooldownActivated)
                    return;

                // todo : play end cooldown animation
                m_IsCooldownActivated = false;
                m_Cooldown.SetActive(false);
                return;
            }

            // play cooldown animation (?)
            m_IsCooldownActivated = true;

            m_Cooldown.SetActive(true);
            m_CooldownCtr.text = cooldown.ToString("0");
        }

        void OnDestroy()
        {
            GameManager.Instance.Owner.SpellHandler.SelectedSpellNet.OnValueChanged -= OnSpellSelected;
        }

        #endregion


        #region Public Manipulators

        public void Update()
        {
            UpdateCooldown();
        }

        #endregion
    }
}