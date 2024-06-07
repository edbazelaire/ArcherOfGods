using Data.GameManagement;
using Enums;
using System.Collections.Generic;
using System;
using TMPro;
using Tools;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Managers.Sound;

namespace Menu.MainMenu.MainTab
{
    public class SoloModeDisplayUI : MObject
    {
        #region Members

        EArenaType          m_ArenaType;
        ArenaData           m_ArenaData;

        TMP_Dropdown        m_DropdownButton;
        GameObject          m_ArenaSection;
        ArenaButton         m_ArenaButton;
        StageSectionUI      m_StageSectionUI;

        #endregion


        #region Init & End

        public override void Initialize()
        {
            base.Initialize();

            m_StageSectionUI.Initialize(m_ArenaData, m_ArenaData.CurrentLevel);
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            SetUpDropDownButton();
            OnArenaTypeChanged(PlayerPrefsHandler.GetArenaType());
        }

        protected override void FindComponents()
        {
            base.FindComponents();

            m_DropdownButton    = Finder.FindComponent<TMP_Dropdown>(gameObject, "DropdownButton");
            m_ArenaSection      = Finder.Find(gameObject, "ArenaSection");
            m_StageSectionUI    = Finder.FindComponent<StageSectionUI>(gameObject, "StageSection");
        }

        #endregion


        #region GUI Manipulators

        void RefreshUI()
        {
            UIHelper.CleanContent(m_ArenaSection);

            m_ArenaButton           = Instantiate(AssetLoader.LoadArenaButton(m_ArenaType), m_ArenaSection.transform).GetComponent<ArenaButton>();
            m_ArenaButton.Initialize(m_ArenaType);
        }

        #endregion


        #region GUI Manipulators

        void SetUpDropDownButton()
        {
            List<string> modes = Enum.GetNames(typeof(EArenaType)).ToList();

            // add values to dropdown
            m_DropdownButton.AddOptions(modes);
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            PlayerPrefsHandler.ArenaTypeChangedEvent += OnArenaTypeChanged;
            m_DropdownButton.onValueChanged.AddListener(OnDropDownValueChanged);
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            PlayerPrefsHandler.ArenaTypeChangedEvent -= OnArenaTypeChanged;
            m_DropdownButton.onValueChanged.RemoveListener(OnDropDownValueChanged);
        }


        void OnArenaTypeChanged(EArenaType arenaType)
        {
            m_ArenaType = arenaType;
            m_ArenaData = AssetLoader.LoadArenaData(arenaType);

            // set value to last selected value
            m_DropdownButton.value = Enum.GetNames(typeof(EArenaType)).ToList().IndexOf(m_ArenaType.ToString());

            RefreshUI();
        }

        void OnDropDownValueChanged(int index)
        {
            if (!Enum.TryParse(m_DropdownButton.options[index].text, out EArenaType arenaType))
            {
                ErrorHandler.Error("Unable to convert " + m_DropdownButton.options[index].text + " as game mode");
                return;
            }

            SoundFXManager.PlayOnce(SoundFXManager.ClickButtonSoundFX);
            PlayerPrefsHandler.SetArenaType(arenaType);
        }

        #endregion
    }
}