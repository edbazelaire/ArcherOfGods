using Enums;
using Game.Managers;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;


public class CharacterSelectionUI : MonoBehaviour
{
    #region Members

    const string c_CharacterSelectionContainer = "CharacterSelectionContainer";

    GameObject m_TemplateCharacterButton;
    GameObject m_ButtonsContainer;

    Dictionary<ECharacter, CharacterButtonUI> m_CharacterButtons;

    #endregion


    #region Initialization

    public void Initialize()
    {
        m_ButtonsContainer = Finder.Find(gameObject, c_CharacterSelectionContainer);
        m_TemplateCharacterButton = AssetLoader.Load<GameObject>("TemplateCharacterButton", AssetLoader.c_MainTab);

        CreateCharacterButtons();
    }

    #endregion


    #region Public Accessors

    public void CreateCharacterButtons()
    {
        // remove all children from the m_CharacterSelectionContainer
        UIHelper.CleanContent(m_ButtonsContainer);

        // reset dict
        m_CharacterButtons = new Dictionary<ECharacter, CharacterButtonUI>();

        // create buttons for each characters
        foreach (ECharacter character in CharacterLoader.Instance.Characters.Keys)
        {
            var characterButton = Instantiate(m_TemplateCharacterButton, m_ButtonsContainer.transform).GetComponent<CharacterButtonUI>();
            characterButton.Initialize(character);
            m_CharacterButtons.Add(character, characterButton);
        }
    }

    /// <summary>
    /// Force selection of a specific character from the outside
    /// </summary>
    /// <param name="character"></param>
    public void SelectCharacter(ECharacter character)
    {
        m_CharacterButtons[character].SelectCharacter();
    }

    #endregion

}
