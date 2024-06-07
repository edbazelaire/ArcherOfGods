using Assets.Scripts.Managers.Sound;
using Enums;
using TMPro;
using Tools;
using UnityEngine.UI;

public class VolumeOptionUI : MObject
{
    #region Members

    EVolumeOption m_Option;

    TMP_Text            m_Title;
    Slider              m_Slider;
    Button              m_MuteButton;
    Image               m_MuteImage;

    #endregion


    #region Init & End

    protected override void FindComponents()
    {
        m_Title         = Finder.FindComponent<TMP_Text>(gameObject, "Title");
        m_Slider        = Finder.FindComponent<Slider>(gameObject, "Slider");
        m_MuteButton   = Finder.FindComponent<Button>(gameObject, "MuteButton");
        m_MuteImage    = Finder.FindComponent<Image>(gameObject, "MuteImage");
    }

    public void Initialize(EVolumeOption option)
    {
        base.Initialize();

        m_Option = option;

        m_Title.text = TextLocalizer.SplitCamelCase(option.ToString());
        m_Slider.value = PlayerPrefsHandler.GetVolume(option);
        m_MuteImage.gameObject.SetActive(PlayerPrefsHandler.GetMuted(option));
    }

    #endregion


    #region Listeners

    protected override void RegisterListeners()
    {
        base.RegisterListeners();

        m_Slider.onValueChanged.AddListener(OnSliderValueChanged);
        m_MuteButton.onClick.AddListener(OnMuteButtonClicked);
    }

    protected override void UnRegisterListeners()
    {
        base.UnRegisterListeners();

        m_Slider.onValueChanged.RemoveAllListeners();
        m_MuteButton.onClick.RemoveAllListeners();
    }

    void OnSliderValueChanged(float value)
    {
        PlayerPrefsHandler.SetVolume(m_Option, value);
        SoundFXManager.RefreshMusicVolume();
    }

    void OnMuteButtonClicked()
    {
        bool wasMuted = PlayerPrefsHandler.GetMuted(m_Option);
        
        m_MuteImage.gameObject.SetActive(!wasMuted);
        PlayerPrefsHandler.SetMuted(m_Option, wasMuted ? 0 : 1);
        SoundFXManager.RefreshMusicVolume();
    }

    #endregion

}
