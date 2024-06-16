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
        bool m_WaitingForReconnection => m_DisconnectedClients.Count > 0;


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
            // Implement game pausing logic here
            Time.timeScale = 0f;
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

        private IEnumerator WaitForHostReconnection()
        {
            Debug.Log("WaitForHostReconnection()");

            float timePassed = Time.unscaledTime - m_DisconnectionTimestamp;
            while (timePassed < m_ReconnectionTimeout)
            {
                if (! m_WaitingForReconnection)
                {
                    // Host has reconnected
                    ResumeGame();
                    yield break;
                }

                ErrorGameUI.SetSubMessage(string.Format(m_CountdownMessage, Mathf.Ceil(m_ReconnectionTimeout - timePassed)));
                yield return null;
            }

            ErrorGameUI.Hide();
            Time.timeScale = 1f;

            // Timeout exceeded, handle accordingly
            GameUIManager.Instance.SetUpGameOver(true);
        }

        private void EndGame()
        {
            int team;

            // both players are disconnected : no winner
            if (m_DisconnectedClients.Count == 0)
            {
                ErrorHandler.Error("EndGame() called by the DeconnectionHandler but no disconnected player were found");
                return;
            }
            else if (m_DisconnectedClients.Count >= 2)
                team = -1;
            else
                team = (GameManager.Instance.Controllers[m_DisconnectedClients[0]].Team + 1) % 2;

            GameManager.Instance.GameOverClientRPC(team);
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

            bool wasWaiting = m_WaitingForReconnection;

            // add client to list of disconnected client
            if (m_DisconnectedClients.Contains(clientId))
                ErrorHandler.Error("Client " + clientId + " already in list of disconected clients");
            else
                m_DisconnectedClients.Add(clientId);

            m_DisconnectionTimestamp = Time.time;

            if (! wasWaiting)
            {
                NotifyPlayers(m_ReconnectionMessage);
                PauseGame();
                StartCoroutine(WaitForHostReconnection());
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