using Enums;
using Menu.Common;
using Menu.Common.Buttons;

namespace Assets.Scripts.Menu.Common.Buttons.TemplateItemButtons
{
    public class TemplateCurrencyIcon : TemplateItemButton
    {
        #region Members

        CollectionFillBar m_CollectionFillBar;

        ERewardType m_RewardType;

        #endregion


        #region Init & End

        public void Initialize(ERewardType reward)
        {
            base.Initialize();

            m_RewardType = reward;

            // set icon and level
            SetUpUI();
        }

        #endregion


        #region GUI Manipulators

        void SetUpUI() 
        { 
            m_LevelValue.text = m_RewardType.ToString();
        }

        #endregion
    }
}