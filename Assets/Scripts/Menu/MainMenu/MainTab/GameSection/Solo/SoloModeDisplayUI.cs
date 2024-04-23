using Assets;
using Data.GameManagement;
using Enums;
using System;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MainMenu.MainTab
{
    public class SoloModeDisplayUI : MObject
    {
        #region Members

        EArenaType  m_ArenaType;
        ArenaData   m_ArenaData;

        TMP_Text m_Title;
        GameObject          m_ArenaSection;
        Button              m_ArenaButton;
        StageSectionUI      m_StageSectionUI;

        #endregion


        #region Init & End

        public override void Initialize()
        {
            base.Initialize();

            m_ArenaData = AssetLoader.LoadArenaData(PlayerPrefsHandler.GetArenaType());
            m_StageSectionUI.Initialize(m_ArenaData, m_ArenaData.CurrentLevel);

            OnArenaTypeChanged(m_ArenaType);
        }

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Title             = Finder.FindComponent<TMP_Text>(gameObject, "Title");
            m_ArenaSection      = Finder.Find(gameObject, "ArenaSection");
            m_StageSectionUI    = Finder.FindComponent<StageSectionUI>(gameObject, "StageSection");
        }

        #endregion


        #region GUI Manipulators

        void RefreshUI()
        {
            UIHelper.CleanContent(m_ArenaSection);

            m_Title.text    = TextLocalizer.SplitCamelCase(m_ArenaType.ToString());
            m_ArenaButton   = Instantiate(AssetLoader.LoadArenaButton(m_ArenaType), m_ArenaSection.transform).GetComponent<Button>();
            m_ArenaButton.onClick.AddListener(OnArenaButtonClicked);
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            PlayerPrefsHandler.ArenaTypeChangedEvent += OnArenaTypeChanged;
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            PlayerPrefsHandler.ArenaTypeChangedEvent -= OnArenaTypeChanged;
        }

        void OnArenaButtonClicked()
        {
            Main.SetPopUp(EPopUpState.ArenaPathScreen, m_ArenaType);
        }

        void OnArenaTypeChanged(EArenaType arenaType)
        {
            m_ArenaType = arenaType;
            m_ArenaData = AssetLoader.LoadArenaData(arenaType);

            RefreshUI();
        }

        #endregion
    }
}