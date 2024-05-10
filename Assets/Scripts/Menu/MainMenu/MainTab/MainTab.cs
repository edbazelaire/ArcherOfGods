using Assets;
using Data.GameManagement;
using Enums;
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

namespace Menu.MainMenu.MainTab
{
    public class MainTab : MainMenuTabContent
    {
        #region Members

        const string            c_CharacterPreviewSection           = "CharacterPreviewSection";
        const string            c_CharacterSelectionPopUp           = "CharacterSelectionPopUp";
        const string            c_PlayButton                        = "PlayButton";
        const string            c_Dropdown                          = "Dropdown";

        CharacterPreviewSectionUI   m_CharacterPreviewSection;
        CharacterSelectionWindow    m_CharacterSelection;
        GameSectionUI               m_GameSectionUI;
        Button                      m_PlayButton;
        Image                       m_PlayButtonImage;
        TMP_Dropdown                m_GameTypeDropDown;

        #endregion


        #region Init & End 

        protected override void FindComponents()
        {
            base.FindComponents(); 

            m_CharacterPreviewSection           = Finder.FindComponent<CharacterPreviewSectionUI>(gameObject, c_CharacterPreviewSection);
            m_CharacterSelection                = Finder.FindComponent<CharacterSelectionWindow>(gameObject, c_CharacterSelectionPopUp);
            m_GameSectionUI                     = Finder.FindComponent<GameSectionUI>(gameObject, "GameSection");
            m_PlayButton                        = Finder.FindComponent<Button>(gameObject, c_PlayButton);
            m_PlayButtonImage                   = Finder.FindComponent<Image>(m_PlayButton.gameObject);
            m_GameTypeDropDown                  = Finder.FindComponent<TMP_Dropdown>(gameObject, c_Dropdown);
        }

        public override void Initialize(TabButton tabButton)
        {
            base.Initialize(tabButton);
                        
            // initialize GameSectionUI
            m_GameSectionUI.Initialize();

            // create all buttons for characters
            m_CharacterSelection.Initialize();
            m_CharacterSelection.gameObject.SetActive(false);

            // set game modes
            SetUpDropDownButton();

            // register to events
            m_PlayButton.onClick.AddListener(OnPlay);

            // initialize character preview section (with a delay to avoid issue with size)
            CoroutineManager.DelayMethod(m_CharacterPreviewSection.Initialize);
            // delay register method by one frame (to avoid issue with Awake() order)
            CoroutineManager.DelayMethod(() => { m_CharacterPreviewSection.CharacterPreviewButton.onClick.AddListener(ToggleCharacterSelection); });

            // register arena level up event
            ArenaData.ArenaLevelCompletedEvent += OnArenaLevelCompleted;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_PlayButton.onClick.RemoveAllListeners();

            // register arena level up event
            ArenaData.ArenaLevelCompletedEvent -= OnArenaLevelCompleted;
        }

        #endregion


        #region Setup UI

        void SetUpDropDownButton()
        {
            List<string> modes = Enum.GetNames(typeof(EGameMode)).ToList();

            m_GameTypeDropDown.AddOptions(modes);

            // change Lobby game mode on new selection
            m_GameTypeDropDown.onValueChanged.AddListener(OnDropDown);

            // set value to last selected value
            m_GameTypeDropDown.value = modes.IndexOf(PlayerPrefsHandler.GetGameMode().ToString());
        }

        #endregion


        #region Activation / Deactivation

        public override void Activate(bool activate)
        {
            base.Activate(activate);

            m_CharacterPreviewSection.Activate(activate);

            if (!activate)
                OnDeactivation();
        }

        void OnDeactivation()
        {
            if (m_CharacterSelection.IsOpened)
                m_CharacterSelection.Close();
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
        public void ToggleCharacterSelection()
        {
            if (m_CharacterSelection == null)
                return;

            if (!m_CharacterSelection.gameObject.activeInHierarchy)
                m_CharacterSelection.Open();
            else
                m_CharacterSelection.Close();
        }

        /// <summary>
        /// Action enabled when the play button is clicked : quick / leave lobby
        /// </summary>
        void OnPlay()
        {
            if (! CharacterBuildsCloudData.IsCurrentBuildOk){
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

        void OnDropDown(int index)
        {
            if (!Enum.TryParse(m_GameTypeDropDown.options[index].text, out EGameMode gameMode))
            {
                ErrorHandler.Error("Unable to convert " + m_GameTypeDropDown.options[index].text + " as game mode");
            }

            GameSectionUI.SetGameMode(gameMode);
        }

        /// <summary>
        /// When an arena is completed, display UI
        /// </summary>
        /// <param name="arenaType"></param>
        /// <param name="level"></param>
        void OnArenaLevelCompleted(EArenaType arenaType, int level)
        {
            Debug.LogWarning("MainTab.OnArenaLevelCompleted");

            var arenaData = AssetLoader.LoadArenaData(arenaType);
            Main.DisplayRewards(arenaData.GetArenaLevelData(level).rewardsData, ERewardContext.ArenaReward);
        }

        #endregion
    }
}
