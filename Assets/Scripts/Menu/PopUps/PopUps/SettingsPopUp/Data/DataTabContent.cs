using Data.GameManagement;
using Menu.MainMenu;
using Menu.PopUps;
using Save;
using System;
using Tools;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class DataTabContent : TabContent
    {
        #region Members

        CloudDataInterfaceUI m_TemplateCloudDataInterfaceUI;
        GameObject  m_Content; 

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_TemplateCloudDataInterfaceUI = AssetLoader.Load<CloudDataInterfaceUI>("CloudDataInterface", AssetLoader.c_SettingsPath);
            m_Content = Finder.Find(gameObject, "Content");
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            UIHelper.CleanContent(m_Content);

            CloudDataInterfaceUI template = Instantiate(m_TemplateCloudDataInterfaceUI, m_Content.transform);
            template.Initialize(null);

            foreach (CloudData manager in Main.CloudSaveManager.CloudData)
            {
                template = Instantiate(m_TemplateCloudDataInterfaceUI, m_Content.transform);
                template.Initialize(manager);
            }
        }

        #endregion


        #region GUI Manipulators

        public override void Activate(bool activate)
        {
            gameObject.SetActive(activate);
        }

        #endregion


        #region Listeners


        #endregion
    }
}