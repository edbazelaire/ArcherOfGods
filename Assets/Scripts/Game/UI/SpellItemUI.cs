using Data;
using Game.Managers;
using Tools;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SpellItemUI 
    {
        #region Members

        const string    c_Icon = "Icon";

        GameObject      m_GameObject;
        Image           m_Image;
        Button          m_Button;
        ESpells         m_Spell;

        #endregion


        #region Constructor

        public SpellItemUI(GameObject gameObject, ESpells eSpells)
        {
            m_GameObject        = gameObject;
            m_Spell             = eSpells;
           
            SetupIcon();
            SetupButton();
        }

        #endregion


        #region Initialization

        public void SetupIcon()
        {
            GameObject icon = Finder.Find(m_GameObject, c_Icon);
            m_Image = icon.GetComponent<Image>();

            SpellData spellData = SpellLoader.Instance.GetSpellData(m_Spell);
            m_Image.sprite = spellData.Image;
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
            GameManager.Instance.CurrentPlayer.SpellHandler.Cast(m_Spell);
        }

        #endregion
    }
}