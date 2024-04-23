﻿using Data;
using Game.Loaders;
using Game.Spells;
using Menu.Common.Infos;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class StateEffectPopUp : PopUp
    {
        #region Members

        // =============================================================================
        // Data
        StateEffect m_StateEffect;

        // =============================================================================
        // GameObjects & Components
        GameObject  m_PropertiesContainer;
        Image       m_Icon;
        TMP_Text    m_Description;

        #endregion


        #region Init & End

        public void Initialize(SStateEffectData stateEffectData, int level)
        {
            m_StateEffect = SpellLoader.GetStateEffect(stateEffectData.StateEffect.ToString(), level);
            m_StateEffect.ApplyStateEffectData(stateEffectData);

            base.Initialize();
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            var iconSection = Finder.Find(m_WindowContent, "IconSection");
            m_Icon = Finder.FindComponent<Image>(iconSection, "Icon");
            m_Description = Finder.FindComponent<TMP_Text>(iconSection, "Description");
            m_PropertiesContainer = Finder.Find(m_WindowContent, "PropertiesContainer");

            m_Title.text = m_StateEffect.StateEffectName;
            m_Icon.sprite = AssetLoader.LoadStateEffectIcon(m_StateEffect.StateEffectName);
            m_Description.text = m_StateEffect.GetDescription();

            InitProperties();
        }

        #endregion


        #region GUI Manipulators

        void InitProperties()
        {
            var templateInfoRow = AssetLoader.Load<GameObject>("InfoRow", AssetLoader.c_MainUIComponentsInfosPath); 

            UIHelper.CleanContent(m_PropertiesContainer);
            foreach (var item in m_StateEffect.GetInfos())
            {
                var infoRow = Instantiate(templateInfoRow, m_PropertiesContainer.transform).GetComponent<SpellInfoRowUI>();
                infoRow.Initialize(item.Key, item.Value);
            }
        }

        #endregion
    }
}