using Enums;
using TMPro;
using Tools;

namespace Menu.Common.Buttons
{
    public class TitleButtonUI : AchievementRewardUI
    {
        #region Members

        TMP_Text       m_Title;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Title = Finder.FindComponent<TMP_Text>(gameObject, "Title");
        }

        public override void Initialize(string value, EAchievementReward ar = EAchievementReward.Title)
        {
            base.Initialize(value, ar);

            m_Title.text = TextHandler.Split(value);
        }

        #endregion


        #region GUI Manipulators


        #endregion
    }
}