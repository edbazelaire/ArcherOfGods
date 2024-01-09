using Enums;
using Game.Managers;
using Managers;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    #region Members

    const string            c_CharacterPreviewContainer         = "CharacterPreviewContainer";
    const string            c_CharacterSelectionContainer       = "CharacterSelectionContainer";
    const string            c_PlayButton                        = "PlayButton";   
    const string            c_Dropdown                          = "Dropdown";   

    public GameObject       CharacterButtonTemplate;
    
    GameObject              m_CharacterPreviewContainer;
    GameObject              m_CharacterSelectionContainer;
    Button                  m_PlayButton;
    Image                   m_PlayButtonImage;
    TMP_Dropdown            m_GameTypeDropDown;

    bool                    m_SearchingLobby;

    Dictionary<ECharacter, CharacterButtonUI> m_CharacterButtons;

    public static Action<int> CharacterSelectedEvent;

    #endregion


    #region Inheritted Manipulators

    // Start is called before the first frame update
    void Start()
    {
        if (CharacterLoader.Instance == null)
        {
            SceneManager.LoadScene(0);
            return;
        }

        m_CharacterPreviewContainer = Finder.Find(c_CharacterPreviewContainer);
        m_CharacterSelectionContainer = Finder.Find(c_CharacterSelectionContainer);
        m_PlayButton = Finder.FindComponent<Button>(c_PlayButton);
        m_PlayButtonImage = Finder.FindComponent<Image>(m_PlayButton.gameObject);
        m_GameTypeDropDown = Finder.FindComponent<TMP_Dropdown>(gameObject, c_Dropdown);

        // create all buttons for characters
        CreateCharacterButtons();

        // set game modes
        SetUpDropDownButton();

        // register to events
        MainMenu.CharacterSelectedEvent += OnCharacterSelected;
        m_PlayButton.onClick.AddListener(OnPlay);

        // select default character
        m_CharacterButtons[PlayerData.Character].SelectCharacter();
    }

    void OnDestroy()
    {
        MainMenu.CharacterSelectedEvent -= OnCharacterSelected;
    }

    #endregion


    #region Private Manipulators

    void CreateCharacterButtons()
    {
        // remove all children from the m_CharacterSelectionContainer
        foreach (Transform child in m_CharacterSelectionContainer.transform)
        {
            Destroy(child.gameObject);
        }

        CleanPreview();

        // reset dict
        m_CharacterButtons = new Dictionary<ECharacter, CharacterButtonUI>();

        // create buttons for each characters
        foreach (ECharacter character in CharacterLoader.Instance.Characters.Keys)
        {
            var characterButton = Instantiate(CharacterButtonTemplate, m_CharacterSelectionContainer.transform);
            m_CharacterButtons.Add(character, new CharacterButtonUI(characterButton, character));
        }
    }

    void CleanPreview()
    {
        foreach (Transform child in m_CharacterPreviewContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void SetUpDropDownButton()
    {
        List<string> modes = new List<string>
        {
            "1v1",
            "2v2"
        };

        m_GameTypeDropDown.AddOptions(modes);

        m_GameTypeDropDown.onValueChanged.AddListener((int index) => { LobbyHandler.Instance.GameMode = m_GameTypeDropDown.options[index].text; });
    }

    #endregion


    #region Event Listeners

    void OnCharacterSelected(int character)
    {
        PlayerData.Character = (ECharacter)character;
        CleanPreview();
        var characterPreview = CharacterLoader.GetCharacterData(PlayerData.Character).InstantiateCharacterPreview(m_CharacterPreviewContainer);

        var baseScale = characterPreview.transform.localScale;
        characterPreview.transform.localScale = new Vector3(baseScale.x * 200f, baseScale.y * 200f, 1f);
    }

    async void OnPlay()
    {
        if ( !m_SearchingLobby)
        {
            // set the button as selected
            m_PlayButtonImage.color = Color.red;
            m_SearchingLobby = await LobbyHandler.Instance.QuickJoinLobby();
            return;
        }

        await LobbyHandler.Instance.LeaveLobby();

        m_SearchingLobby = false;
        m_PlayButtonImage.color = Color.white;
    }

    void OnLobbyCompleted()
    {
        SceneLoader.Instance.LoadScene("Arena");
    }

    #endregion
}
