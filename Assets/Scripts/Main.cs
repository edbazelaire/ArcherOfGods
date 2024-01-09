using Game;
using Game.Managers;
using Managers;
using System.Collections;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Assets
{
    public class Main : MonoBehaviour
    {
        bool m_CharacterLoaderReady = false;
        bool m_SpellLoaderReady = false;
        bool m_PlayerDataReady = false;

        bool m_Ready => m_CharacterLoaderReady && m_SpellLoaderReady && m_PlayerDataReady;

        // Use this for initialization
        async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
                m_PlayerDataReady = true;
                PlayerData.PlayerName = AuthenticationService.Instance.PlayerName;
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        void Update()
        {
            m_CharacterLoaderReady = CharacterLoader.Instance != null;
            m_SpellLoaderReady = SpellLoader.Instance != null;

            if (!m_Ready)
                return;

            SceneLoader.Instance.LoadScene("MainMenu");
        }
    }
}