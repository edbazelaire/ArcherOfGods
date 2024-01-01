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

        /// <summary> name of the GameObject containing the IconImage </summary>
        const string    c_IconImage     = "IconImage";
        /// <summary> name of the GameObject containing the border </summary>
        const string    c_Border        = "Border";
        /// <summary> name of the GameObject containing the cooldown </summary>
        const string    c_Cooldown      = "Cooldown";
        /// <summary> name of the GameObject containing the cooldown counter </summary>
        const string    c_CooldownCtr   = "CooldownCtr";

        /// <summary> GameObject of the SpellItemUI created by the GameManagerUI </summary>
        GameObject      m_GameObject;
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
        ESpells         m_Spell;
        /// <summary> Is the cooldown activated ? </summary>
        bool            m_IsCooldownActivated;

        #endregion


        #region Constructor

        public SpellItemUI(GameObject gameObject, ESpells spell)
        {
            m_GameObject        = gameObject;
            m_Spell             = spell;

            SetupIcon();
            SetupButton();

            // register to spell selection changes to update the border if needed
            GameManager.Instance.Owner.SpellHandler.SelectedSpellNet.OnValueChanged += OnSpellSelected;
            // register to cooldown changes to update the cooldown if needed
            GameManager.Instance.Owner.SpellHandler.CooldownsNet.OnListChanged += OnCooldownChanged;
        }

        #endregion


        #region Initialization

        /// <summary>
        /// Setup the icon of the spell in the SpellItemContainer
        /// </summary>
        public void SetupIcon()
        {
            m_IconImage     = Finder.FindComponent<Image>(m_GameObject, c_IconImage);
            m_Border        = Finder.FindComponent<Image>(m_GameObject, c_Border);
            m_Cooldown      = Finder.Find(m_GameObject, c_Cooldown);
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
            m_Button = Finder.FindComponent<Button>(m_GameObject);
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
            m_Border.color = (ESpells)newValue == m_Spell ? Color.red : Color.black;
        }

        /// <summary>
        /// When the cooldown changes, update the cooldown if needed
        /// </summary>
        /// <param name="changeEvent"></param>
        void OnCooldownChanged(NetworkListEvent<float> changeEvent)
        {
            // only check for the spell we are interested in
            if (changeEvent.Index != GameManager.Instance.Owner.SpellHandler.GetSpellIndex(m_Spell))
                return;

            float cooldown = changeEvent.Value;
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

        /// <summary>
        /// When the object is destroyed, unregister from events
        /// </summary>
        void OnDestroy()
        {
            GameManager.Instance.Owner.SpellHandler.SelectedSpellNet.OnValueChanged -= OnSpellSelected;
            GameManager.Instance.Owner.SpellHandler.CooldownsNet.OnListChanged -= OnCooldownChanged;
        }

        #endregion
    }
}