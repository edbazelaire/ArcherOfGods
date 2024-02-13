using Game;
using Network;
using TMPro;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class EndGameUI : MonoBehaviour
{
    #region Members

    const string c_TitleText = "TitleText";
    const string c_LeaveButton = "LeaveButton";

    TMP_Text m_TitleText;
    Button m_LeaveButton;

    #endregion


    // Use this for initialization
    public void SetUpGameOver(bool win)
    {
        InitializeUI();

        m_TitleText.text = win ? "Victory" : "Defeat";
        m_TitleText.color = win ? Color.green : Color.red;

        m_LeaveButton.onClick.AddListener(Leave);
    }

    void InitializeUI()
    {
        m_TitleText = Finder.FindComponent<TMP_Text>(gameObject, c_TitleText);
        m_LeaveButton = Finder.FindComponent<Button>(gameObject, c_LeaveButton);
    }

    void Leave()
    {
        // reset network manager
        NetworkManager.Singleton.Shutdown();

        // reset GameManager
        GameManager.Instance.Shutdown();

        // load MainMenu
        SceneLoader.Instance.LoadScene("MainMenu");
    }
}
