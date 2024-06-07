using Enums;
using System;
using Tools;
using UnityEngine;

namespace Menu.MainMenu.MainTab
{
    public class GameSectionUI : MObject
    {
        #region Members

        EGameMode m_GameMode;
        GameObject m_Content;

        
        #endregion


        #region Init & End

        public override void Initialize()
        {
            base.Initialize();

            // init UI
            if (!Enum.TryParse(PlayerPrefs.GetString("GameMode"), out m_GameMode))
            {
                ErrorHandler.Error("Unable to parse GameMode : " + PlayerPrefs.GetString("GameMode"));
                SetGameMode(EGameMode.Arena);
            } 
            else
            {
                OnGameModeChangedEvent(m_GameMode);
            }
        }

        protected override void FindComponents()
        {
            m_Content = Finder.Find(gameObject, "Content");
        }

        
        #endregion


        #region GUI Manipulators

        void RefreshGameModeUI()
        {
            // clean previous content
            UIHelper.CleanContent(m_Content);

            switch (m_GameMode)
            {
                case (EGameMode.Arena):
                    SoloModeDisplayUI gameModeUI = GameObject.Instantiate(AssetLoader.Load<GameObject>("SoloModeDisplay", AssetLoader.c_SoloModeUIPath), m_Content.transform).GetComponent<SoloModeDisplayUI>();
                    gameModeUI.Initialize();
                    break;

                case (EGameMode.Multi):
                    break;
            }
           
        }

        #endregion


        #region Static Methods

        public static void SetGameMode(EGameMode gameMode)
        {
            PlayerPrefsHandler.SetGameMode(gameMode);
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            PlayerPrefsHandler.GameModeChangedEvent += OnGameModeChangedEvent;
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            PlayerPrefsHandler.GameModeChangedEvent -= OnGameModeChangedEvent;
        }


        void OnGameModeChangedEvent(EGameMode gameMode)
        {
            m_GameMode = gameMode;

            RefreshGameModeUI();
        }

        #endregion
    }
}