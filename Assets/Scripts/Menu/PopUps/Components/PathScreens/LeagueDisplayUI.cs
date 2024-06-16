using Assets;
using Data.GameManagement;
using Enums;
using Save;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class LeagueDisplayUI : MObject
    {
        #region Members

        ELeague m_League;

        LeagueStageDisplayUI        m_TemplateStageDisplayUIPrefab;

        GameObject                  m_Content;

        GameObject                  m_LeagueBanner;
        List<LeagueStageDisplayUI>  m_Stages;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_TemplateStageDisplayUIPrefab = AssetLoader.Load<LeagueStageDisplayUI>("LeagueStageDisplay", AssetLoader.c_UIPath + "OverlayScreens/Components/RewardsPath/LeaguePathContent/");
            m_Content = gameObject;
        }

        public void Initialize(ELeague league)
        {
            m_League = league;

            base.Initialize();
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            UIHelper.CleanContent(m_Content);
            SetupBanner();
            SetupStagesDisplay();
        }

        #endregion


        #region GUI Manipulators

        void SetupBanner()
        {
            m_LeagueBanner =  Instantiate(AssetLoader.Load<GameObject>("LeagueBanner", AssetLoader.c_UIPath + "OverlayScreens/Components/RewardsPath/LeaguePathContent/"), m_Content.transform);
            Finder.FindComponent<Image>(m_LeagueBanner, "LeagueBannerIcon").sprite = AssetLoader.LoadLeagueBanner(m_League);
        }

        void SetupStagesDisplay()
        {
            m_Stages = new List<LeagueStageDisplayUI>();
            for (int i = 0; i < Main.LeagueDataConfig.GetLeagueData(m_League).LevelData.Count; i++)
            {
                var stageDisplayUI = Instantiate(m_TemplateStageDisplayUIPrefab, m_Content.transform);
                stageDisplayUI.Initialize(m_League, i);
                m_Stages.Add(stageDisplayUI);
            }
        }

        public float GetOffsetX()
        {
            // init position with current position
            float poseX = 0;

            // move stages until current stage level is left of viewport
            for (int i = 0; i < ProgressionCloudData.CurrentLeagueLevel; i++)
            {
                poseX -= Finder.FindComponent<RectTransform>(m_Stages[i].gameObject).rect.width;
            }

            // remove half of current stage size
            poseX -= Finder.FindComponent<RectTransform>(m_Stages[ProgressionCloudData.CurrentLeagueLevel].gameObject).rect.width / 2;
            return poseX;
        }

        #endregion
    }
}