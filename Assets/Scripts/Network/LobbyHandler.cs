using Assets.Scripts.Network;
using Enums;
using Game;
using Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class LobbyHandler : MonoBehaviour
    {
        static LobbyHandler s_Instance;
        public static LobbyHandler Instance { 
            get { 
                if (s_Instance != null) 
                    return s_Instance;
                var gameObject = new GameObject("LobbyHandler");
                gameObject.AddComponent<LobbyHandler>();
                s_Instance = gameObject.GetComponent<LobbyHandler>();
                s_Instance.Initialize();
                return s_Instance;
            } 
        }

        const string KEY_RELAY_CODE = "RelayCode";

        public Action<ulong, ECharacter> OnRelayJoined;

        private Lobby m_HostLobby;
        private Lobby m_JoinedLobby;

        private string m_GameMode = "1v1";

        private float heartbeatTimer    = 0.0f;
        private float updateLobbyTimer  = 0.0f;

        bool m_ServerStarted = false;
        bool m_SceneLoaded = false;
        bool m_ClientConnected = false;
        bool m_LobbyGetLobbyRequestInProgress = false;

        bool IsHost => m_HostLobby != null && m_JoinedLobby != null && m_HostLobby.Id == m_JoinedLobby.Id;
        int m_MaxPlayers => m_GameMode == "1v1" ? 1 : 2;
        public int MaxPlayers => m_MaxPlayers;


        public string GameMode { get => m_GameMode; set => m_GameMode = value; }

        private void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneChanged;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            HandleLobbyHeartbeat();

            CheckLobbyFull();
            HandleLobbyUpdates();
        }

        public async Task<bool> QuickJoinLobby()
        {
            m_ServerStarted = false;

            bool success = await JoinLobby();

            if (!success)
                success = await CreateLobby();

            if (! success)
                Debug.Log("Failed to join or create lobby");

            return success;
        }

        public async Task<bool> CreateLobby()
        {
            try
            {
                string lobbyName = "Lobby_" + UnityEngine.Random.Range(0, 99);

                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player
                    {
                        Data = PlayerData.GetPlayerData()
                    },
                    Data = new Dictionary<string, DataObject> {
                        { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "1v1", DataObject.IndexOptions.S1) },
                        { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "", DataObject.IndexOptions.S2) }
                    }
                };  

                m_HostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, m_MaxPlayers, createLobbyOptions);
                m_JoinedLobby = m_HostLobby;

                Debug.Log("Lobby created: " + m_HostLobby.Id);

                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("Failed to create lobby: " + e.Message);
            }

            return false;
        }

        /// <summary>
        /// Join the first lobby found
        /// </summary>
        public async Task<bool> JoinLobby()
        {
            try
            {
                var lobbies = await ListLobbies();
                if (lobbies.Count == 0)
                    return false;

                JoinLobbyByIdOptions lobbyOptions = new JoinLobbyByIdOptions
                {
                    Player = new Player
                    {
                        Data = PlayerData.GetPlayerData()
                    }
                };

                m_JoinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbies[0].Id, lobbyOptions);
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("Failed to join lobby: " + e.Message);
            }

            return false;
        }

        public async Task<List<Lobby>> ListLobbies()
        {
            try
            {
                QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
                    Count = 25,
                    Filters = new List<QueryFilter> {
                        new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                        new QueryFilter(QueryFilter.FieldOptions.S1, "1v1", QueryFilter.OpOptions.EQ)
                    },
                    Order = new List<QueryOrder> {
                        new QueryOrder(false, QueryOrder.FieldOptions.Created)
                    }
                };

                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

                Debug.Log("Lobbies found: " + queryResponse.Results.Count);

                foreach (var lobby in queryResponse.Results)
                {
                    Debug.Log("Lobby: " + lobby.Name + " - MaxPlayers : " + lobby.MaxPlayers);
                }

                return queryResponse.Results;

            } catch (LobbyServiceException e)
            {
                Debug.Log("Failed to list lobbies: " + e.Message);
                return new List<Lobby>();
            }
        }

        async void HandleLobbyHeartbeat()
        {
            if (m_HostLobby == null)
                return;

            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer > 0.0f)
                return;

            // reset heatbeat
            heartbeatTimer = 15.0f;

            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(m_HostLobby.Id);

            } catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
                heartbeatTimer = 0f;        // set timer back to 0 to send an other one
            }
            
           
        }

        async void HandleLobbyUpdates()
        {
            if (m_JoinedLobby == null || m_ClientConnected || m_LobbyGetLobbyRequestInProgress)
                return;

            updateLobbyTimer -= Time.deltaTime;
            if (updateLobbyTimer > 0.0f)
                return;

            updateLobbyTimer = 1.1f;
            m_LobbyGetLobbyRequestInProgress = true;

            m_JoinedLobby = await LobbyService.Instance.GetLobbyAsync(m_JoinedLobby.Id);

            m_LobbyGetLobbyRequestInProgress = false;

            if (( m_JoinedLobby.Data[KEY_RELAY_CODE].Value != "") && !m_ServerStarted)
            {
                m_ServerStarted = true;

                if (!IsHost)
                    await RelayHandler.Instance.JoinRelay(m_JoinedLobby.Data[KEY_RELAY_CODE].Value);

                m_ClientConnected = true;
                Debug.Log("Joining done");
            }
        }

        void OnClientConnected(ulong clientId)
        {
            Debug.Log("LobbyManager : Client connected : " + clientId);

            if (NetworkManager.Singleton.LocalClientId != clientId)
                return;

            var playerData = m_PlayerData;
            GameManager.Instance.AddPlayerDataServerRPC(clientId, (ECharacter)Convert.ToInt16(playerData[PlayerData.KEY_CHARACTER].Value));
        }

        void CheckLobbyFull()
        {
            if (m_JoinedLobby == null || m_SceneLoaded)
                return;

            if (m_JoinedLobby.Players.Count != m_JoinedLobby.MaxPlayers)
                return;

            Debug.Log("Lobby full : game can start");
            m_SceneLoaded = true;
            SceneLoader.Instance.LoadScene("Arena");
        }

        async void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Arena" || m_ServerStarted)
                return;

            if ( m_HostLobby == null )
                return;

            SceneManager.sceneLoaded -= OnSceneChanged;

            bool success = await CheckSceneLoaded(scene);
            if (!success)
                return;

            string relayCode = await RelayHandler.Instance.CreateRelay();

            UpdateLobbyRelayCode(relayCode);
        }

        async Task<bool> CheckSceneLoaded(Scene scene)
        {
            float timer = 30f;
            int checkMilliSec = 1000;
            while (scene.isLoaded == false)
            {
                await Task.Delay(checkMilliSec);
                timer -= (float)checkMilliSec / 1000;

                Debug.Log("Waiting for scene to load... ");

                if (timer <= 0)
                {
                    return false;
                } 
            }

            Debug.LogWarning("Scene loaded");
            return true;
        }


        #region Update Data Methods 

        async void UpdateLobbyRelayCode(string relayCode)
        {
            try
            {
                m_HostLobby = await Lobbies.Instance.UpdateLobbyAsync(
                    m_HostLobby.Id, 
                    new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject> {
                            { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                        }
                    }
                );
            } catch (LobbyServiceException e)
            {
                Debug.Log("Failed to update lobby: " + e.Message);
            }
        }

        async void UpdateLobbyGameMode(string gameMode)
        {
            try
            {
                m_HostLobby = await Lobbies.Instance.UpdateLobbyAsync(
                    m_HostLobby.Id, 
                    new UpdateLobbyOptions
                    {
                        Data = new Dictionary<string, DataObject> {
                            { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                        }
                    }
                );
            } catch (LobbyServiceException e)
            {
                Debug.Log("Failed to update lobby: " + e.Message);
            }
        }

        async void UpdatePlayerData(string playerName = "", ECharacter character = ECharacter.Count, ulong clientId = 99999999)
        {
            var data = new Dictionary<string, PlayerDataObject>();
            if (playerName != "")
                data.Add(PlayerData.KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName));
            if ( character != ECharacter.Count )
                data.Add(PlayerData.KEY_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ((int)character).ToString()));
            if ( clientId != 99999999)
                data.Add(PlayerData.KEY_CLIENT_ID, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, clientId.ToString()));

            try
            {
                await LobbyService.Instance.UpdatePlayerAsync(
                    m_JoinedLobby.Id, 
                    AuthenticationService.Instance.PlayerId, 
                    new UpdatePlayerOptions { Data = data }
                );
            } catch (LobbyServiceException e)
            {
                Debug.Log("Failed to update player name: " + e.Message);
            }
        }

        #endregion

        public async Task LeaveLobby()
        {
            await LobbyService.Instance.RemovePlayerAsync(m_JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
            m_JoinedLobby = null;
        }

        async void MigradeLobbyHost()
        {
            try
            {
                m_HostLobby = await Lobbies.Instance.UpdateLobbyAsync(m_HostLobby.Id, new UpdateLobbyOptions
                {
                    HostId = m_JoinedLobby.Players[1].Id
                });
            } catch (LobbyServiceException e)
            {
                Debug.Log("Failed to migrate lobby host: " + e.Message);
            }
        }

        async void DeleteLobby()
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(m_HostLobby.Id);
                m_HostLobby = null;
            } catch (LobbyServiceException e)
            {
                Debug.Log("Failed to delete lobby: " + e.Message);
            }
        }

        Dictionary<string, PlayerDataObject> m_PlayerData
        {
            get
            {
                foreach (var player in m_JoinedLobby.Players)
                {
                    if (player.Id == AuthenticationService.Instance.PlayerId)
                    {
                        return player.Data;
                    }
                }

                return null;
            }
        }

        #region Debug Methods

        void PrintPlayers(Lobby lobby)
        {
            Debug.Log("Players in lobby : " + lobby.Id);
            foreach (var player in lobby.Players)
            {
                Debug.Log("Player: " + player.Id + " with name " + player.Data["PlayerName"].Value);
            }
        }

        #endregion
    }
}