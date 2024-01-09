using Data;
using Enums;
using Game;
using Game.Managers;
using System;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonUI
{
    const string c_Icon         = "Icon";
    const string c_Border       = "Border";

    GameObject m_GameObject;
    ECharacter m_Character;

    Image m_Border;
    Image m_Icon;

    public CharacterButtonUI(GameObject gameObject, ECharacter character)
    {
        m_GameObject = gameObject;
        m_Character = character;

        SetUpIcon();
        SetUpButton();

        MainMenu.CharacterSelectedEvent += OnCharacterSelected;
    }

    public void SelectCharacter()
    {
        MainMenu.CharacterSelectedEvent?.Invoke((int)m_Character);
    }

    void SetUpIcon()
    {
        CharacterData data = CharacterLoader.GetCharacterData(m_Character);

        m_Icon = Finder.FindComponent<Image>(m_GameObject, c_Icon);
        m_Icon.sprite = data.Image;

        m_Border = Finder.FindComponent<Image>(m_GameObject, c_Border);
        // todo
    }

    void SetUpButton()
    {
        Button button = m_GameObject.GetComponent<Button>();
        button.onClick.AddListener(SelectCharacter);
        return;
    }

    void OnCharacterSelected(int character)
    {
        if (character == (int)m_Character)
        {
            m_Border.color = Color.red;
        }
        else
        {
            m_Border.color = Color.white;
        }
    }
}
