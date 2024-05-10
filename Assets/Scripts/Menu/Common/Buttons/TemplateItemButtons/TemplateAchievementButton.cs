using Data;
using Enums;
using Save;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Common.Buttons
{
    public class TemplateAchievementButton : MObject
    {
        #region Members

        // ==============================================================================================
        // Data
        AchievementData m_Achievement;

        // ==============================================================================================
        // GameObjects & Components
        Button              m_Button;
        TMP_Text            m_Title;
        CollectionFillBar   m_FillBar;

        public Button Button => m_Button;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Button    = Finder.FindComponent<Button>(gameObject);
            m_Title     = Finder.FindComponent<TMP_Text>(gameObject, "Title");
            m_FillBar   = Finder.FindComponent<CollectionFillBar>(gameObject);
        }
        
        public void Initialize(AchievementData achievement)
        {
            m_Achievement = achievement;

            base.Initialize();
        }

        protected override void SetUpUI()
        {
            m_Title.text = TextLocalizer.SplitCamelCase(m_Achievement.name);
            m_FillBar.Initialize(m_Achievement.Count, m_Achievement.RequestedValue);
        }

        #endregion


        #region GUI Manipulators

        void RefreshUI()
        {
            m_FillBar.UpdateCollection(m_Achievement.Count, m_Achievement.RequestedValue);
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            m_Button.onClick.AddListener(OnClicked);
            StatCloudData.AnalyticsDataChanged += OnAnalyticsDataChanged;
        }


        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();

            if (m_Button == null)
                return;

            m_Button.onClick.RemoveAllListeners();
            StatCloudData.AnalyticsDataChanged -= OnAnalyticsDataChanged;
        }

        /// <summary>
        /// Action happening when the button is clicked : open the achievement
        /// info popup
        /// </summary>
        void OnClicked()
        {
            // ===========================================
            // TODO : OPEN POPUP
            // ===========================================

            if (! m_Achievement.IsUnlockable)
            {
                Debug.LogWarning("Achivement not unlockable");
                return;
            }

            m_Achievement.Unlock();

            RefreshUI();
        }

        /// <summary>
        /// Refresh UI of the template when the Annalytics linked to this
        /// achievement is updated in the cloud data
        /// </summary>
        /// <param name="analytics"></param>
        void OnAnalyticsDataChanged(EAnalytics analytics)
        {
            if (m_Achievement.Analytics != analytics)
                return;

            RefreshUI();
        }

        #endregion
    }
}