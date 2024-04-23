using Data.GameManagement;
using Enums;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu.PopUps
{
    public class ArenaPathScreen : OverlayScreen
    {
        #region Members

        ArenaData m_ArenaData;

        GameObject m_ScrollContent;
        GameObject m_Viewport;
        StageDisplayUI m_StageDisplayUIPrefab;

        List<StageDisplayUI> m_Stages;

        #endregion


        #region Init & End

        public void Initialize(EArenaType arenaType)
        {
            m_ArenaData = AssetLoader.LoadArenaData(arenaType);

            base.Initialize();
        }

        protected override void FindComponents()
        {
            base.FindComponents();

            m_StageDisplayUIPrefab = AssetLoader.Load<StageDisplayUI>("StageDisplay", AssetLoader.c_UIPath + "OverlayScreens/Components/ArenaPathContent/");

            m_ScrollContent = Finder.Find(gameObject, "ScrollContent");
            m_Viewport = Finder.Find(gameObject, "Viewport");
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            SetupStagesDisplay();
        }

        protected override void OnInitializationCompleted()
        {
            base.OnInitializationCompleted();
            CoroutineManager.DelayMethod(CenterOnCurrentStage, 2);
        }

        #endregion


        #region GUI Manipulators

        void SetupStagesDisplay()
        {
            UIHelper.CleanContent(m_ScrollContent);
            m_Stages = new List<StageDisplayUI>();
            
            for (int i = 0; i < m_ArenaData.MaxLevel; i++)
            {
                var stageDisplayUI = Instantiate(m_StageDisplayUIPrefab, m_ScrollContent.transform);
                stageDisplayUI.Initialize(m_ArenaData, i);
                m_Stages.Add(stageDisplayUI);
            }
        }

        void CenterOnCurrentStage()
        {
            if (m_ArenaData.CurrentLevel == 0)
                return;

            // init position with current position
            float poseX = 0;

            // move stages until current stage level is left of viewport
            for (int i = 0; i < m_ArenaData.CurrentLevel; i++)
            {
                poseX -= Finder.FindComponent<RectTransform>(m_Stages[i].gameObject).rect.width;
            }

            // add half of viewport size
            poseX += (Finder.FindComponent<RectTransform>(m_Viewport).rect.width / 2);

            // remove half of current stage size
            poseX -= Finder.FindComponent<RectTransform>(m_Stages[m_ArenaData.CurrentLevel].gameObject).rect.width / 3;

            // poseX sup 0 means that not enought stages are behind to be able to center current stage
            if (poseX > 0)
            {
                return;
            }

            // setup position
            m_ScrollContent.transform.localPosition = new Vector3(poseX, m_ScrollContent.transform.localPosition.y, 0f);
        }

        #endregion
    }
}