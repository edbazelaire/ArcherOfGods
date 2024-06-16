using Tools;
using UnityEngine;
using Save;
using Assets;

namespace Menu.MainMenu.MainTab
{
    public class RankedModeDisplayUI : MObject
    {
        #region Members

        GameObject              m_ButtonSection;
        LeagueBannerButton      m_LeagueBannerButton;
        LeagueStageSectionUI    m_StageSectionUI;

        #endregion


        #region Init & End

        public override void Initialize()
        {
            base.Initialize();

            m_StageSectionUI.Initialize(ProgressionCloudData.CurrentLeague, ProgressionCloudData.CurrentLeagueLevel);
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            RefreshUI();
        }

        protected override void FindComponents()
        {
            base.FindComponents();

            m_ButtonSection     = Finder.Find(gameObject, "ButtonSection");
            m_StageSectionUI    = Finder.FindComponent<LeagueStageSectionUI>(gameObject, "StageSection");
        }

        #endregion


        #region GUI Manipulators

        void RefreshUI()
        {
            UIHelper.CleanContent(m_ButtonSection);

            m_LeagueBannerButton = Instantiate(AssetLoader.LoadLeagueButton(), m_ButtonSection.transform).GetComponent<LeagueBannerButton>();
            m_LeagueBannerButton.Initialize(ProgressionCloudData.CurrentLeague);
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

        }
      
        #endregion
    }
}