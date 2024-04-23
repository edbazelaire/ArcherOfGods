using Enums;
using Tools;

namespace Menu.Common.Buttons
{
    public class TemplateRuneItemUI : TemplateCollectableItemUI
    {
        #region Members

        // ========================================================================================
        // Button Data
        protected ERune m_Rune => (ERune)m_Collectable;

        #endregion


        #region GUI Manipulators

        /// <summary>
        /// Setup icon of the spell and color of its border
        /// </summary>
        public virtual void RefreshRune(ERune rune)
        {
            base.Initialize(rune);
        }

        #endregion
    }
}