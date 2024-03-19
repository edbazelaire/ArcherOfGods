using Enums;
using Game.Managers;
using Menu.Common.Buttons;
using System.Collections.Generic;
using Tools;
using UnityEngine;


public class CharacterSelectionUI : MonoBehaviour
{
    #region Members

    const string c_CharacterSelectionContainer = "CharacterSelectionContainer";

    GameObject m_TemplateCharacterButton;
    GameObject m_ButtonsContainer;

    Dictionary<ECharacter, TemplateCharacterItemUI> m_CharacterButtons;

    #endregion


    #region Initialization

    public void Initialize()
    {
        // check if a specific container for the buttons was provided, otherwise use this parent as container
        m_ButtonsContainer = Finder.Find(gameObject, c_CharacterSelectionContainer, throwError: false);
        if (m_ButtonsContainer == null)
            m_ButtonsContainer = gameObject;

        m_TemplateCharacterButton = AssetLoader.LoadTemplateItem("CharacterItem");

        CreateCharacterButtons();
    }

    #endregion


    #region Public Accessors

    public void CreateCharacterButtons()
    {
        // remove all children from the m_CharacterSelectionContainer
        UIHelper.CleanContent(m_ButtonsContainer);

        // reset dict
        m_CharacterButtons = new Dictionary<ECharacter, TemplateCharacterItemUI>();

        // create buttons for each characters
        foreach (ECharacter character in CharacterLoader.Instance.Characters.Keys)
        {
            var characterButton = Instantiate(m_TemplateCharacterButton, m_ButtonsContainer.transform).GetComponent<TemplateCharacterItemUI>();
            characterButton.Initialize(character);
            m_CharacterButtons.Add(character, characterButton);
        }
    }

    #endregion

}
