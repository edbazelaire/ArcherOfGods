using Assets.Scripts.Managers.Sound;
using Enums;
using System;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class DebugOptionUI : MObject
{
    #region Members

    EDebugOption    m_Option;
    Action          m_OnActivate;
    Action          m_OnDestroy;

    [SerializeField] Color m_ActivatedColor; 
    [SerializeField] Color m_DeactivatedColor; 

    TMP_Text            m_Title;
    Button              m_ActivateButton;
    Button              m_DestroyButton;
    Image               m_ActivateButtonImage;


    #endregion


    #region Init & End

    protected override void FindComponents()
    {
        m_Title                 = Finder.FindComponent<TMP_Text>(gameObject, "Title");
        m_ActivateButton        = Finder.FindComponent<Button>(gameObject, "ActivateButton");
        m_DestroyButton         = Finder.FindComponent<Button>(gameObject, "DestroyButton");
        m_ActivateButtonImage   = Finder.FindComponent<Image>(m_ActivateButton.gameObject);
    }

    public void Initialize(EDebugOption option, Action OnActivate, Action OnDestroy)
    {
        base.Initialize();

        m_Option = option;
        m_OnActivate = OnActivate;
        m_OnDestroy = OnDestroy;

        m_Title.text = TextLocalizer.SplitCamelCase(option.ToString());
        RefreshActivationColor();
    }

    #endregion


    #region GUI Manipulators

    void RefreshActivationColor()
    {
        m_ActivateButtonImage.color = PlayerPrefsHandler.GetDebug(m_Option) ? m_ActivatedColor : m_DeactivatedColor;
    }

    #endregion


    #region Listeners

    protected override void RegisterListeners()
    {
        base.RegisterListeners();

        m_ActivateButton.onClick.AddListener(OnActivateButtonClicked);
        m_DestroyButton.onClick.AddListener(OnDestroyButtonClicked);
    }

    protected override void UnRegisterListeners()
    {
        base.UnRegisterListeners();

        m_ActivateButton.onClick.RemoveAllListeners();
        m_DestroyButton.onClick.RemoveAllListeners();
    }

    void OnActivateButtonClicked()
    {
        m_OnActivate();
        RefreshActivationColor() ;
    }

    void OnDestroyButtonClicked()
    {
        m_OnDestroy();
    }

    #endregion

}
