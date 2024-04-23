using Data;
using Menu.Common.Infos;
using System.Collections.Generic;
using Tools;
using static UnityEngine.Rendering.DebugUI;

namespace Menu.PopUps
{
    public class SpellInfoPopUp : CollectableInfoPopUp
    {
        #region Members

        // =========================================================================================
        // GameObjects & Components
        StateEffectsInfoRow                 m_StateEffectsInfoRow;

        // =========================================================================================
        // Dependent Members
        SpellData m_SpellData       => m_Data as SpellData;
        bool m_IsLinked             => m_SpellData.Linked;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_StateEffectsInfoRow = Finder.FindComponent<StateEffectsInfoRow>(gameObject, "StateEffectsInfoRow");
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            SetUpStateEffects();
        }

        #endregion



        #region UIManipulators

        protected override void SetUpButtons()
        {
            if (m_IsLinked)
            {
                m_UpgradeButton.gameObject.SetActive(false);
                return;
            }

            base.SetUpButtons();
        }

        /// <summary>
        /// Display infos of the spell
        /// </summary>
        protected override void SetUpInfoRow(string key, object value, object newDataValue = null)
        {
            if (key == "Effects")
                return;
            
            base.SetUpInfoRow(key, value, newDataValue);
        }

        void SetUpStateEffects()
        {
            var spellData = m_Data.GetInfos();
            List<SStateEffectData> effectsData = spellData.ContainsKey("Effects") ? spellData["Effects"] as List<SStateEffectData> : new List<SStateEffectData>();

            if (effectsData.Count == 0)
            {
                m_StateEffectsInfoRow.gameObject.SetActive(false);
                return;
            }

            m_StateEffectsInfoRow.gameObject.SetActive(true);
            m_StateEffectsInfoRow.Initialize(effectsData, m_Level);
        }

        protected override void RefreshUpgradeButtonUI()
        {
            if (m_IsLinked)
                return;

            base.RefreshUpgradeButtonUI();
        }

        #endregion
    }
}