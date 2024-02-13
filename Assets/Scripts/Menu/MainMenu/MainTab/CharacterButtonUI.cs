using Data;
using Enums;
using Game.Managers;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonUI: MonoBehaviour
{
    const string c_Icon         = "Icon";
    const string c_Border       = "Border";

    ECharacter m_Character;

    Image m_Border;
    Image m_Icon;

    public void Initialize(ECharacter character)
    {
        m_Character = character;

        SetUpIcon();
        SetUpButton();

        MainMenuUI.CharacterSelectedEvent += OnCharacterSelected;
    }

    private void OnDestroy()
    {
        MainMenuUI.CharacterSelectedEvent -= OnCharacterSelected;
    }

    public void SelectCharacter()
    {
        MainMenuUI.CharacterSelectedEvent?.Invoke(m_Character);
    }

    void SetUpIcon()
    {
        CharacterData data = CharacterLoader.GetCharacterData(m_Character);

        m_Icon = Finder.FindComponent<Image>(gameObject, c_Icon);
        m_Icon.sprite = data.Image;

        m_Border = Finder.FindComponent<Image>(gameObject, c_Border);
        // todo
    }

    void SetUpButton()
    {
        Button button = Finder.FindComponent<Button>(gameObject);
        button.onClick.AddListener(SelectCharacter);
        return;
    }

    void OnCharacterSelected(ECharacter character)
    {
        if (m_Border == null)
        {
            Debug.LogWarning("Error : no border found for " + m_Character);
            return;
        }

        if (character == m_Character)
        {
            m_Border.color = Color.red;
        }
        else
        {
            m_Border.color = Color.white;
        }
    }
}
