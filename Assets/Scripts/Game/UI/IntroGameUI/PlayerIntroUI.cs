using Managers;
using Menu.Common.Buttons;
using Menu.MainMenu;
using TMPro;
using Tools;
using UnityEngine;

namespace Game.UI
{
    public class PlayerIntroUI : MObject
    {
        #region Members

        ProfileDisplayUI    m_ProfileDisplayUI;

        GameObject          m_CharacterDisplayUI;
        GameObject          m_CharacterContainer;
        TMP_Text            m_Level;
        GameObject          m_RuneContainer;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_ProfileDisplayUI = Finder.FindComponent<ProfileDisplayUI>(gameObject);

            m_CharacterDisplayUI    = Finder.Find(gameObject, "CharacterDisplay");
            m_CharacterContainer    = Finder.Find(m_CharacterDisplayUI, "CharacterContainer");
            m_Level                 = Finder.FindComponent<TMP_Text>(m_CharacterDisplayUI, "Level");
            m_RuneContainer         = Finder.Find(m_CharacterDisplayUI, "RuneContainer");
        }

        public void Initialize(SPlayerData playerData)
        {
            base.Initialize();

            m_ProfileDisplayUI.Initialize(playerData.ProfileData);

            UIHelper.SpawnCharacter(playerData.Character, m_CharacterContainer, "Overlay");
            m_Level.text = "Level " + playerData.CharacterLevel;

            UIHelper.CleanContent(m_RuneContainer);
            var rune = Instantiate(AssetLoader.LoadTemplateItem(playerData.Rune), m_RuneContainer.transform).GetComponent<TemplateRuneItemUI>();
            rune.Initialize(playerData.Rune);
        }

        #endregion
    }
}