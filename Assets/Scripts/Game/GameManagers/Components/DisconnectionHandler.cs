using Game.UI;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.GameManagers.Components
{
    public class DisconnectionHandler : NetworkBehaviour
    {
        [SerializeField] float      m_ReconnectionTimeout   = 5f; // Timeout period in seconds
        [SerializeField] string     m_ReconnectionMessage   = "Your opponent has been disconnected"; 
        [SerializeField] string     m_CountdownMessage      = "The game will end in... {0}"; 

        List<ulong> m_DisconnectedClients;
        float       m_DisconnectionTimestamp;
        bool m_IsWaitingForReconnection => m_DisconnectedClients.Count > 0;
        bool m_IsHostDisconnected => m_DisconnectedClients.Contains(NetworkManager.ServerClientId);


        #region Init & End

        public void Start()
        {

            m_DisconnectedClients = new();

            Debug.Log("Added listeners");
            NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (NetworkManager.Singleton == null) 
                return;

            NetworkManager.Singleton.OnClientConnectedCallback  -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        #endregion


        #region Handling Deconnection

        private void PauseGame()
        {
            // stop time scale
            Time.timeScale = 0f;

            // notify player of the error
            NotifyPlayers(m_ReconnectionMessage);

            // start coroutine waiting for reconnection
            StartCoroutine(WaitForClientReconnection());
        }

        private void ResumeGame()
        {
            ErrorGameUI.Hide();

            // Implement game resuming logic here
            Time.timeScale = 1f;
        }

        private void NotifyPlayers(string message)
        {
            // Implement player notification logic here
            Debug.LogWarning(message);

            ErrorGameUI.Display(message);
        }

        private IEnumerator WaitForClientReconnection()
        {
            Debug.Log("WaitForHostReconnection()");

            float timePassed = Time.unscaledTime - m_DisconnectionTimestamp;
            while (timePassed < m_ReconnectionTimeout)
            {
                timePassed = Time.unscaledTime - m_DisconnectionTimestamp;
                if (! m_IsWaitingForReconnection)
                {
                    // Host has reconnected
                    ResumeGame();
                    yield break;
                }

                ErrorGameUI.SetSubMessage(string.Format(m_CountdownMessage, Mathf.Ceil(m_ReconnectionTimeout - timePassed)));
                yield return null;
            }

            ErrorGameUI.Hide();

            // end the game
            EndGame();

            // resume to the game
            Time.timeScale = 1f;
        }

        private void EndGame()
        {
            // force deactivation of the Intro 
            if (GameUIManager.IntroGameUI != null && GameUIManager.IntroGameUI.isActiveAndEnabled) 
            {
                GameUIManager.IntroGameUI.Deactivate();
            }

            // no host - insta display end of game
            if (m_IsHostDisconnected)
            {
                GameUIManager.Instance.SetUpGameOver(true);
                return;
            }

            // host still alive - shutdown game then display end of game
            int winningTeam;

            // both players are disconnected : no winner
            if (m_DisconnectedClients.Count == 0)
            {
                ErrorHandler.Error("EndGame() called by the DeconnectionHandler but no disconnected player were found");
                return;
            }
            else if (m_DisconnectedClients.Count >= 2)
                winningTeam = -1;
            else
                winningTeam = m_DisconnectedClients[0] != GameManager.Instance.Owner.PlayerId ? GameManager.Instance.Owner.Team : (GameManager.Instance.Owner.Team + 1) % 2;

            GameManager.Instance.GameOverClientRPC(winningTeam);
        }

        #endregion


        #region Listeners

        /// <summary>
        /// Called when a client disconnects
        /// </summary>
        /// <param name="clientId"></param>
        void OnClientDisconnected(ulong clientId)
        {
            Debug.Log("OnClientDisconnected : " + clientId);

            if (GameManager.IsGameOver)
                return;

            switch (GameManager.Instance.State)
            {
                default:
                    break;
            }

            bool wasWaiting = m_IsWaitingForReconnection;

            // add client to list of disconnected client
            if (m_DisconnectedClients.Contains(clientId))
                ErrorHandler.Error("Client " + clientId + " already in list of disconected clients");
            else
                m_DisconnectedClients.Add(clientId);

            m_DisconnectionTimestamp = Time.unscaledTime;
            
            if (NetworkManager.ServerClientId == clientId)
            {
                // HOST DISCONNECTED
                Debug.Log("HOST HAS BEEN DISCONNECTED");
            }

            if (! wasWaiting)
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Called when a client re-connects
        /// </summary>
        /// <param name="clientId"></param>
        void OnClientConnected(ulong clientId) 
        {
            Debug.Log("OnClientConnected : " + clientId);

            if (GameManager.IsGameOver || !GameManager.Instance.IsGameLoaded)
                return;

            if (! m_DisconnectedClients.Contains(clientId))
                ErrorHandler.Error("Client " + clientId + " not found in list of disconected clients");
            else
                m_DisconnectedClients.Remove(clientId);
        }

        #endregion
    }
}