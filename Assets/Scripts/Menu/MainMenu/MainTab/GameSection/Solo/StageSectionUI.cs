using Data.GameManagement;
using Enums;
using System.Collections.Generic;
using TMPro;
using Tools;
using Tools.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MainMenu.MainTab
{
    public class StageSectionUI : MObject
    {
        #region Members

        const string CURRENT_STAGE_ANIMATION = "CURRENT_STAGE_ANIMATION";

        // ================================================================================
        // Serialized Data
        [SerializeField] GameObject m_Knob;
        [SerializeField] GameObject m_Line;

        [SerializeField] Color ColorCompleted;
        [SerializeField] Color ColorCurrent;
        [SerializeField] Color ColorNotDone;

        // ================================================================================
        // GameObejcts & Components
        GameObject  m_PathDisplayContainer;
        TMP_Text    m_CurrentStageDisplay;

        // ================================================================================
        // Data
        ArenaData   m_ArenaData;
        int         m_ArenaLevel;
        List<Image> m_Knobs;

        #endregion

        #region Init & End

        public void Initialize(ArenaData arenaData, int arenaLevel)
        {
            base.Initialize();

            m_ArenaData = arenaData;
            m_ArenaLevel = arenaLevel;
            RefreshUI();
        }

        protected override void FindComponents()
        {
            base.FindComponents();

            m_PathDisplayContainer = Finder.Find(gameObject, "PathDisplayContainer");
            m_CurrentStageDisplay = Finder.FindComponent<TMP_Text>(gameObject, "CurrentStageText");
        }

        #endregion


        #region GUI Manipulators

        void RefreshUI()
        {
            RefreshTitle();
            ResetPathDisplay();
        }

        void RefreshTitle()
        {
            m_CurrentStageDisplay.text = GetLevelString();
        }

        void ResetPathDisplay()
        {
            UIHelper.CleanContent(m_PathDisplayContainer);
            m_Knobs = new List<Image>();

            int nStages = m_ArenaData.GetArenaLevelData(m_ArenaLevel).StageData.Count;
            for (int i = 0; i < nStages; i++)
            {
                m_Knobs.Add(Instantiate(m_Knob, m_PathDisplayContainer.transform).GetComponent<Image>());
                SetKnobColor(i);
            }
        }

        void SetKnobColor(int index)
        {
            // Check Arena Level first
            if (m_ArenaLevel < m_ArenaData.CurrentLevel)
            {
                m_Knobs[index].color = ColorCompleted;
                return;
            }

            if (m_ArenaLevel > m_ArenaData.CurrentLevel)
            {
                m_Knobs[index].color = ColorNotDone;
                return;
            }

            // Check index of CURRENT STAGE
            if (index < m_ArenaData.CurrentStage)
                m_Knobs[index].color = ColorCompleted;
            else if (index == m_ArenaData.CurrentStage)
            {
                m_Knobs[index].color = ColorCurrent;

                var pulse = m_Knobs[index].gameObject.AddComponent<Pulse>();
                pulse.Initialize(CURRENT_STAGE_ANIMATION, -1f, 0.9f, 1.1f, pulseDuration: 1.5f, pauseDuration: 0f);
            }
            else 
                m_Knobs[index].color = ColorNotDone;
        }

        #endregion


        #region Animations



        #endregion


        #region Helpers 

        string GetLevelString()
        {
            return "Stage " + (m_ArenaLevel + 1).ToString();
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            PlayerPrefsHandler.ArenaTypeChangedEvent    += OnArenaTypeChanged;
            ArenaData.ArenaLevelCompletedEvent          += OnArenaLevelCompleted;
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            PlayerPrefsHandler.ArenaTypeChangedEvent    -= OnArenaTypeChanged;
            ArenaData.ArenaLevelCompletedEvent          -= OnArenaLevelCompleted;
        }

        void OnArenaTypeChanged(EArenaType arenaType)
        {
            m_ArenaData = AssetLoader.LoadArenaData(arenaType);
            m_ArenaLevel = m_ArenaData.CurrentLevel;
            RefreshUI();
        }

        void OnArenaLevelCompleted(EArenaType arenaType, int level) 
        { 
            ResetPathDisplay();
        }

        #endregion
    }
}