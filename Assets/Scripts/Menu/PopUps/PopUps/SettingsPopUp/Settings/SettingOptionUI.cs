using Data.GameManagement;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class SettingOptionUI : MonoBehaviour
{
    #region Memebers

    ESettings           m_Option;
    Color               m_DefaultColor;

    Image               m_Border;
    TMP_Text            m_NameText;
    TMP_InputField      m_InputField;

    #endregion


    #region Init & End

    void FindComponents()
    {
        m_Border = Finder.FindComponent<Image>(gameObject);
        m_NameText = Finder.FindComponent<TMP_Text>(gameObject, "Name");
        m_InputField = Finder.FindComponent<TMP_InputField>(gameObject, "InputField");
    }

    public void Initialize(ESettings option)
    {
        FindComponents();

        m_Option = option;
        m_DefaultColor = m_Border.color;

        m_NameText.text = TextLocalizer.SplitCamelCase(option.ToString()).Replace(" Factor", "");
        m_InputField.text = Settings.Get(option).ToString("F2");

        m_InputField.onDeselect.AddListener(OnInputFieldValueChanged); 
    }

    #endregion


    #region Listeners

    void OnInputFieldValueChanged(string rawValue)
    {
        if (rawValue.Contains("."))
            rawValue = rawValue.Replace(".", ",");

        if(float.TryParse(rawValue, out float value))
        {
            if (value > 0)
            {
                Settings.Set(m_Option, value);
                m_Border.color = m_DefaultColor;
                return;
            }
        }

        ErrorHandler.Warning("Bad input value : " + rawValue);
        m_Border.color = Color.red;
    }

    #endregion

}
