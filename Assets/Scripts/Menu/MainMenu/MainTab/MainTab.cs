using Assets;
using Enums;
using Game.Managers;
using Managers;
using Menu.MainMenu;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class MainTab : MainMenuTabContent
    {
        #region Members

        const string            c_CharacterPreviewContainer         = "CharacterPreviewContainer";
        const string            c_CharacterSelection                = "CharacterSelection";
        const string            c_PlayButton                        = "PlayButton";
        const string            c_Dropdown                          = "Dropdown";

        GameObject              m_CharacterPreviewContainer;
        Button                  m_CharacterPreviewButton;
        RectTransform           m_CharacterPreviewRectTransform;
        CharacterSelectionUI    m_CharacterSelection;
        Button                  m_PlayButton;
        Image                   m_PlayButtonImage;
        TMP_Dropdown            m_GameTypeDropDown;


        public static Action<ECharacter> CharacterSelectedEvent;

        #endregion


        #region Init & End 

        public override void Initialize(TabButton tabButton)
        {
            base.Initialize(tabButton);

            if (Main.Instance == null)
            {
                ErrorHandler.Warning("Main Instance not found : reloading release scene");
                SceneManager.LoadScene("Release");
                return;
            }

            m_CharacterPreviewContainer         = Finder.Find(c_CharacterPreviewContainer);
            m_CharacterPreviewButton            = Finder.FindComponent<Button>(m_CharacterPreviewContainer);
            m_CharacterPreviewRectTransform     = Finder.FindComponent<RectTransform>(m_CharacterPreviewContainer);
            m_CharacterSelection                = Finder.FindComponent<CharacterSelectionUI>(c_CharacterSelection);
            m_PlayButton                        = Finder.FindComponent<Button>(c_PlayButton);
            m_PlayButtonImage                   = Finder.FindComponent<Image>(m_PlayButton.gameObject);
            m_GameTypeDropDown                  = Finder.FindComponent<TMP_Dropdown>(gameObject, c_Dropdown);

            // create all buttons for characters
            m_CharacterSelection.Initialize();
            m_CharacterSelection.gameObject.SetActive(false);

            // set game modes
            SetUpDropDownButton();

            // register to events
            CharacterSelectedEvent += OnCharacterSelected;
            m_PlayButton.onClick.AddListener(OnPlay);
            m_CharacterPreviewButton.onClick.AddListener(ToggleCharacterSelection);

            // init with Player Pref data
            CoroutineManager.DelayMethod(SetupDefaultPlayerData);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CharacterSelectedEvent -= OnCharacterSelected;
            m_PlayButton.onClick.RemoveAllListeners();
            m_CharacterPreviewButton.onClick.RemoveAllListeners();
        }

        #endregion


        #region Setup UI

        /// <summary>
        /// Set Menu with default player data
        /// </summary>
        private void SetupDefaultPlayerData()
        {
            m_CharacterSelection.SelectCharacter(PlayerData.Character);
        }


        void CleanPreview()
        {
            UIHelper.CleanContent(m_CharacterPreviewContainer);
        }

        void SetUpDropDownButton()
        {
            List<string> modes = Enum.GetNames(typeof(EGameMode)).ToList<string>();

            m_GameTypeDropDown.AddOptions(modes);

            // change Lobby game mode on new selection
            m_GameTypeDropDown.onValueChanged.AddListener((int index) => {
                Enum.TryParse(m_GameTypeDropDown.options[index].text, out EGameMode gameMode);
                LobbyHandler.Instance.GameMode = gameMode;
            });
        }

        #endregion


        #region Lobby

        async void JoinLobby()
        {
            Main.SetState(EAppState.Lobby);

            // set the button as selected
            m_PlayButtonImage.color = Color.red;
            await LobbyHandler.Instance.QuickJoinLobby();
        }

        void LeaveLobby()
        {
            LobbyHandler.Instance.LeaveLobby();
            Main.SetState(EAppState.MainMenu);
            m_PlayButtonImage.color = Color.white;
        }

        #endregion


        #region Event Listeners

        /// <summary>
        /// When a character is seleted :
        ///     - change PlayerData
        ///     - setup new Preview 
        /// </summary>
        /// <param name="character"></param>
        void OnCharacterSelected(ECharacter character)
        {
            // setup selected character in player data
            PlayerData.Character = character;

            // clean current preview
            CleanPreview();

            // get selected character preview
            var characterData = CharacterLoader.GetCharacterData(PlayerData.Character);
            var characterPreview = characterData.InstantiateCharacterPreview(m_CharacterPreviewContainer);

            // display character preview
            var baseScale = characterPreview.transform.localScale * characterData.Size;
            float scaleFactor = 0.6f * m_CharacterPreviewRectTransform.rect.height / characterPreview.transform.localScale.y;
            characterPreview.transform.localScale = new Vector3(baseScale.x * scaleFactor, baseScale.y * scaleFactor, 1f);
        }

        /// <summary>
        /// Activate / Deactivate character selection
        /// </summary>
        [Command(KeyCode.A)]
        public void ToggleCharacterSelection()
        {
            if (!Checker.NotNull(m_CharacterSelection))
                return;

            m_CharacterSelection.gameObject.SetActive(!m_CharacterSelection.gameObject.activeInHierarchy);
        }

        /// <summary>
        /// Action enabled when the play button is clicked : quick / leave lobby
        /// </summary>
        void OnPlay()
        {
            // if L
            if (Main.State == EAppState.Lobby)
            {
                LeaveLobby();
                return;
            }

            JoinLobby();
        }

        #endregion
    }
}
