using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    GameObject m_ButtonsContainer;
    private Button m_ServerBtn;
    private Button m_HostBtn;
    private Button m_ClientBtn;

    GameObject m_NetworkDisplayer;
    TMP_Text m_NetworkDisplayerText;

    private void Awake()
    {
        // setup buttons
        m_ButtonsContainer = Finder.Find("ButtonsContainer");
        m_ServerBtn = Finder.FindComponent<Button>(m_ButtonsContainer, "ServerBtn");
        m_HostBtn = Finder.FindComponent<Button>(m_ButtonsContainer, "HostBtn");
        m_ClientBtn = Finder.FindComponent<Button>(m_ButtonsContainer, "ClientBtn");

        // set up network displayer
        m_NetworkDisplayer = Finder.Find("NetworkDisplayer");
        m_NetworkDisplayerText = Finder.FindComponent<TMP_Text>(m_NetworkDisplayer, "Text");
        m_NetworkDisplayer.SetActive(false);

        m_ServerBtn.onClick.AddListener(() =>
        {
            OnClickedButton("Server");
        });

        m_HostBtn.onClick.AddListener(() =>
        {
            OnClickedButton("Host");
        });

        m_ClientBtn.onClick.AddListener(() =>
        {
            OnClickedButton("Client");
        });
    }

    void OnClickedButton(string bname)
    {
        if (bname == "Server")
        {
            NetworkManager.Singleton.StartServer();
        }
        else if (bname == "Host")
        {
            NetworkManager.Singleton.StartHost();
        }
        else if (bname == "Client")
        {
            NetworkManager.Singleton.StartClient();
        } else
        {
            Debug.LogError($"Button {bname} not implemented");
        }

        m_ButtonsContainer.SetActive(false);

        // Active display
        m_NetworkDisplayer.SetActive(true);
        m_NetworkDisplayerText.text = bname;

        if (bname == "Client")
        {
            m_NetworkDisplayerText.color = Color.green;
        }   

    }
}
