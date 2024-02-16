﻿using System.Collections;
using Tools;
using UnityEngine;

namespace Menu.MainMenu
{
    public class MainMenuTabContent : TabContent
    {
        #region Members

        RectTransform m_RectTransform;
        public RectTransform RectTransform => m_RectTransform;

        #endregion


        #region Init & End
      
        public void SetWidth(float width)
        {
            m_RectTransform = Finder.FindComponent<RectTransform>(gameObject);
            m_RectTransform.rect.Set(m_RectTransform.rect.x, m_RectTransform.rect.y, width, m_RectTransform.rect.height);
        }

        #endregion

    }
}