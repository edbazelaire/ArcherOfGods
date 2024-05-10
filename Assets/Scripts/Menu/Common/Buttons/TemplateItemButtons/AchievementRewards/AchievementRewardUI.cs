using Enums;
using Save;
using Tools;
using UnityEngine.UI;

namespace Menu.Common.Buttons
{
    public class AchievementRewardUI : MObject
    {
        #region Members

        // Data
        protected string                m_Name;
        protected EAchievementReward    m_AchievementReward;

        // GameObjects & Components
        protected Image                 m_Selected;
        protected Button                m_Button;

        // Public Accessors
        public string               Name                => m_Name;
        public EAchievementReward   AchievementReward   => m_AchievementReward;
        public Button               Button              => m_Button;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Button        = Finder.FindComponent<Button>(gameObject);
            m_Selected      = Finder.FindComponent<Image>(gameObject, "Selected", false);
        }

        public virtual void Initialize(string name, EAchievementReward aR)
        {
            base.Initialize();

            m_Name = name;
            m_AchievementReward = aR;

            SetSelected(false);
        }
        
        #endregion


        #region BUI Manipulators

        public void SetSelected(bool selected)
        {
            if (m_Selected != null)
                m_Selected.gameObject.SetActive(selected);
        }

        #endregion


        #region Button Methods

        public virtual void SetAsCurrent()
        {
            if (ProfileCloudData.CurrentProfileData[m_AchievementReward].Contains(m_Name) && m_Name != "None")
                return;

            ProfileCloudData.SetCurrentData(m_AchievementReward, m_Name);
        }

        public virtual void Unlock() 
        {
            ProfileCloudData.AddAchievementReward(m_AchievementReward, m_Name);
        }

        #endregion


    }
}