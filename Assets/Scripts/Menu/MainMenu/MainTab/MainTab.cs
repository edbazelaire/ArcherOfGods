using Assets;
using Enums;
using Menu.MainMenu;
using Network;
using Save;
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

        const string            c_CharacterPreviewSection           = "CharacterPreviewSection";
        const string            c_CharacterSelectionPopUp           = "CharacterSelectionPopUp";
        const string            c_PlayButton                        = "PlayButton";
        const string            c_Dropdown                          = "Dropdown";

        CharacterPreviewSectionUI   m_CharacterPreviewSection;
        CharacterSelectionUI        m_CharacterSelection;
        Button                      m_PlayButton;
        Image                       m_PlayButtonImage;
        TMP_Dropdown                m_GameTypeDropDown;

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

            m_CharacterPreviewSection           = Finder.FindComponent<CharacterPreviewSectionUI>(gameObject, c_CharacterPreviewSection);
            m_CharacterSelection                = Finder.FindComponent<CharacterSelectionUI>(gameObject, c_CharacterSelectionPopUp);
            m_PlayButton                        = Finder.FindComponent<Button>(gameObject, c_PlayButton);
            m_PlayButtonImage                   = Finder.FindComponent<Image>(m_PlayButton.gameObject);
            m_GameTypeDropDown                  = Finder.FindComponent<TMP_Dropdown>(gameObject, c_Dropdown);

            // initialize character preview section (with a delay to avoid issue with size)
            CoroutineManager.DelayMethod(m_CharacterPreviewSection.Initialize);

            // create all buttons for characters
            m_CharacterSelection.Initialize();
            m_CharacterSelection.gameObject.SetActive(false);

            // set game modes
            SetUpDropDownButton();

            // register to events
            m_PlayButton.onClick.AddListener(OnPlay);

            // delay register method by one frame (to avoid issue with Awake() order)
            CoroutineManager.DelayMethod(() => { m_CharacterPreviewSection.CharacterPreviewButton.onClick.AddListener(ToggleCharacterSelection); });
        }

        public override void Activate(bool activate)
        {
            base.Activate(activate);

            m_CharacterPreviewSection.Activate(activate);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_PlayButton.onClick.RemoveAllListeners();
        }

        #endregion


        #region Setup UI

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
            if (!CharacterBuildsCloudData.IsCurrentBuildOk)
            {
                Main.ErrorMessagePopUp("Current build is not valid");
                return;
            }

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
