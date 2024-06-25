using Enums;
using Menu.MainMenu.MainTab.GameSection.Training;
using System;
using Tools;
using UnityEngine;

namespace Menu.MainMenu.MainTab
{
    public class GameSectionUI : MObject
    {
        #region Members

        EGameMode m_GameMode => PlayerPrefsHandler.GetGameMode();
        GameObject m_Content;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            m_Content = Finder.Find(gameObject, "Content");
        }

        public override void Initialize()
        {
            base.Initialize();

            OnGameModeChangedEvent(m_GameMode);
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
                    ArenaModeDisplayUI arenaModeUI = GameObject.Instantiate(AssetLoader.Load<ArenaModeDisplayUI>("ArenaModeDisplay", AssetLoader.c_ArenaModeUIPath), m_Content.transform);
                    arenaModeUI.Initialize();
                    break;

                case (EGameMode.Ranked):
                    RankedModeDisplayUI rankedModeUI = GameObject.Instantiate(AssetLoader.Load<RankedModeDisplayUI>("RankedModeDisplay", AssetLoader.c_RankedModeUIPath), m_Content.transform);
                    rankedModeUI.Initialize();
                    break;

                case (EGameMode.Training):
                    TrainingModeDisplayUI trainingModeUI = GameObject.Instantiate(AssetLoader.Load<TrainingModeDisplayUI>(AssetLoader.c_GameSectionPath), m_Content.transform);
                    trainingModeUI.Initialize();
                    break;

                default:
                    ErrorHandler.Error("Unhandled game mode : " + m_GameMode);
                    return;
            }
           
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
            RefreshGameModeUI();
        }

        #endregion
    }
}