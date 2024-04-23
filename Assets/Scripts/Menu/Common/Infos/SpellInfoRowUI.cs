﻿using Enums;
using System.Text.RegularExpressions;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Common.Infos
{
    public class SpellInfoRowUI : InfoRowUI
    {
        #region Members

        Image       m_Icon;
        TMP_Text    m_Name;
        TMP_Text    m_Value;
        TMP_Text    m_BonusValue;

        bool m_IsPerc;

        #endregion


        #region Init & End

        void FindComponents()
        {
            var iconContainer   = Finder.Find(gameObject, "IconContainer");
            m_Icon              = Finder.FindComponent<Image>(iconContainer, "Icon");

            //var infosContainer  = Finder.Find(gameObject, "InfosContainer");
            m_Name              = Finder.FindComponent<TMP_Text>(gameObject, "Name");
            m_Value             = Finder.FindComponent<TMP_Text>(gameObject, "Value");
            m_BonusValue        = Finder.FindComponent<TMP_Text>(gameObject, "BonusValue");
        }


        public void Initialize(string name, object value, object newValue = null, bool isPerc = false)
        {
            FindComponents();

            // set name of the row
            m_Name.text = TextLocalizer.SplitCamelCase(TextLocalizer.LocalizeText(name));
            // by default, deactivate bonus value
            m_BonusValue.gameObject.SetActive(false);
            // is the value a percentage value ?
            m_IsPerc = isPerc;

            // handles special cases
            switch (name)
            {
                case "Type":
                    m_Icon.sprite = AssetLoader.LoadUIElementIcon(value as string);
                    m_Value.text = TextLocalizer.SplitCamelCase(TextLocalizer.LocalizeText(value as string));
                    return; 
            }

            m_Icon.sprite = AssetLoader.LoadUIElementIcon(name);

            RefreshValue(value, newValue);
        }

        #endregion


        #region GUI Manipulators

        public void RefreshValue(object value, object newValue = null)
        {
            // not a float : skip
            if (!float.TryParse(value.ToString(), out float floatValue))
                return;

            // try and parse newValue into a float if provided
            float? floatNewValue = null;
            if (newValue != null)
            {
                if (!float.TryParse(newValue.ToString(), out float temp))
                    ErrorHandler.Error("Unable to parse new value (" + newValue.ToString() + ") of property " + name + " into a float");
                else
                    floatNewValue = temp;
            }

            RefreshValue(floatValue, floatNewValue);
        }

        public void RefreshValue(float value, float? newValue = null)
        {
            if (m_IsPerc)
                m_Value.text = Mathf.Round(value * 100).ToString("0") + "%";
            else 
                m_Value.text = value.ToString(Mathf.Round(value) == value ? "0" : "F2");

            if (newValue == null || newValue.Value - value == 0)
            {
                m_BonusValue.gameObject.SetActive(false);
                return;
            }

            float bonus = newValue.Value - value;
            m_BonusValue.gameObject.SetActive(true);
            m_BonusValue.text = (bonus > 0 ? "+ " : "- ") + Mathf.Abs(bonus).ToString(Mathf.Round(bonus) == bonus ? "0" : "F2");
            m_BonusValue.color = bonus > 0 ? Color.green : Color.red;
        }

        #endregion
    }
}