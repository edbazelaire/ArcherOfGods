using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button m_ServerBtn;
    [SerializeField] private Button m_HostBtn;
    [SerializeField] private Button m_ClientBtn;

    private void Awake()
    {
        m_ServerBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        m_HostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        m_ClientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
}