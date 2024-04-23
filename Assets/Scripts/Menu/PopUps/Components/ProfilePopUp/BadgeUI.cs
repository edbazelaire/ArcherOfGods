using Enums;
using System;
using System.Collections;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps.Components.ProfilePopUp
{
    public class BadgeUI : MObject
    {
        #region Members

        // =================================================================================
        // Data
        EBadge  m_Badge;
        ELeague m_League;

        // =================================================================================
        // GameObjects & Components
        Button  m_Button;
        Image   m_Icon;
        Image   m_Selected;

        // =================================================================================
        // Dependent Properties
        public Button  Button => m_Button;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Button    = Finder.FindComponent<Button>(gameObject);
            m_Icon      = Finder.FindComponent<Image>(gameObject, "Icon");
            m_Selected  = Finder.FindComponent<Image>(gameObject, "Selected");
        }

        public void Initialize((EBadge, ELeague) badgeData)
        {
            Initialize(badgeData.Item1, badgeData.Item2);
        }

        public void Initialize(EBadge badge, ELeague league)
        {
            base.Initialize();

            SetSelected(false);
            RefreshUI(badge, league);
        }

        #endregion


        #region GUI Manipulators

        public void RefreshUI((EBadge, ELeague) badgeData)
        {
            RefreshUI(badgeData.Item1, badgeData.Item2);
        }

        public void RefreshUI(EBadge badge, ELeague rarety)
        {
            m_Badge     = badge;
            m_League    = rarety;

            m_Icon.sprite = AssetLoader.LoadBadgeIcon(badge, rarety);
        }

        public void SetSelected(bool selected)
        {
            m_Selected.gameObject.SetActive(selected);
        }

        #endregion
    }
}