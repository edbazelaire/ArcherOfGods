using Game;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PlayerBarUI : MonoBehaviour
{
    const string c_Fill = "Fill";
    const string c_Text = "Text";

    Image       m_Fill;
    TMP_Text    m_Text;
    int         m_MaxValue;
    int         m_CurrentValue;

    public void Initialize(int currentValue, int maxValue)
    {
        m_Fill = Finder.FindComponent<Image>(gameObject, c_Fill);
        m_Text = Finder.FindComponent<TMP_Text>(gameObject, c_Text, throwError: false);

        SetMaxValue(maxValue);
        SetValue(currentValue);
        UpdateChanges();
    }

    public void OnValueChanged(int oldValue, int newValue)
    {
        SetValue(newValue);
        UpdateChanges();
    }

    public void OnMaxValueChanged(int oldValue, int newValue)
    {
        SetMaxValue(newValue);
        UpdateChanges();
    }

    void SetMaxValue(int value)
    {
        m_MaxValue = value;
    }

    void SetValue(int value)
    {
        m_CurrentValue = value;
    }

    void UpdateChanges()
    {
        m_Fill.fillAmount = (float)m_CurrentValue / m_MaxValue;
        if (m_Text != null)
            m_Text.text = $"{m_CurrentValue} / {m_MaxValue}";
    }

}