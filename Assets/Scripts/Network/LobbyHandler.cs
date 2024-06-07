using Assets;
using Assets.Scripts.Network;
using Enums;
using Game;
using Managers;
using Menu.MainMenu.MainTab;
using Save;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tools;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Network
{
    public enum ELobbyState
    {
        Inactive,
        Joining,
        WaitingLobbyFull,
        SceneLoading,
        WaitingRelayCode,
        JoiningRelay,
        WaitingGameManager,
        SendingPlayerData,
        Ready,
    }

    public class LobbyHandler : MonoBehaviour
    {
        #region Members

        static LobbyHandler s_Instance;
        public static LobbyHandler Instance { 
            get 
            { 
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
        const float HEARTBEAT_TIMER = 15f;
        const float UPDATE_LOBBY_TIMER = 1.5f;

        public Action<ulong, ECharacter> OnRelayJoined;

        private ELobbyState m_State;
        private Lobby       m_HostLobby;
        private Lobby       m_JoinedLobby;

        private EGameMode m_GameMode = EGameMode.Arena;
        private EArenaType m_ArenaType = EArenaType.FireArena;

        private float m_HeartbeatTimer    = 0.0f;
        private float m_UpdateLobbyTimer  = 0.0f;

        bool m_RequestInProgress = false;

        bool IsHost => m_HostLobby != null && m_JoinedLobby != null && m_HostLobby.Id == m_JoinedLobby.Id;
        int m_MaxPlayers => m_GameMode == EGameMode.Arena ? 1 : 2;
        public int MaxPlayers => m_MaxPlayers;
        public ELobbyState State => m_State;

        public EGameMode GameMode { get => m_GameMode; set => m_GameMode = value; }
        public EArenaType ArenaType { get => m_ArenaType; set => m_ArenaType = value; }

        #endregion


        #region Initialize & End

        private void Initialize()
        {
            PlayerPrefsHandler.GameModeChangedEvent     += OnGameModeChanged;
            PlayerPrefsHandler.ArenaTypeChangedEvent    += OnArenaTypeChanged;

            m_GameMode = PlayerPrefsHandler.GetGameMode();
            m_ArenaType = PlayerPrefsHandler.GetArenaType();

            DontDestroyOnLoad(gameObject);
        }

        
        /// <summary>
        /// Leave current lobby and reset parameterss
        /// </summary>
        void ResetLobby()
        {
            m_HostLobby = null;
            m_JoinedLobby = null;

            SetState(ELobbyState.Inactive);
        }

        #endregion


        #region Update Methods

        void Update()
        {
            HandleLobbyState();
        }

        /// <summary>
        /// Check lobby data updates
        /// </summary>
        async void HandleLobbyState()
        {
            if (m_State == ELobbyState.Inactive)
                return;

            if (m_RequestInProgress)
                return;

            // send heartbeat to maintain the lobby alive
            HandleLobbyHeartbeat();

            switch (m_State)
            {
                case ELobbyState.Joining:
                    if (m_JoinedLobby == null)
                        return;

                    SetRequestInProgress();

                    m_JoinedLobby = await LobbyService.Instance.GetLobbyAsync(m_JoinedLobby.Id);
                    NextState();
                    return;

                case ELobbyState.WaitingLobbyFull:
                    UpdateLobbyData();

                    if (m_JoinedLobby.Players.Count != m_JoinedLobby.MaxPlayers)
                        return;
                    SceneLoader.Instance.LoadScene("Arena");
                    NextState();
                    return;

                case ELobbyState.SceneLoading:
                    if (SceneLoader.Instance.IsLoading)
                        return;

                    if (!IsHost)
                    {
                        NextState();
                        return;
                    }

                    // Host creates the relay and instantly go to state "SendingPlayerData"
                    SetRequestInProgress();

                    string relayCode = await RelayHandler.Instance.CreateRelay();

                    UpdateLobbyRelayCode(relayCode);

                    // spawn the GameManager on Server
                    while (!GameManager.FindInstance())
                        return;

                    SetState(ELobbyState.SendingPlayerData);
                    return;

                case ELobbyState.WaitingRelayCode:
                    UpdateLobbyData();

                    // if relay code not provided yet : return
                    if (m_JoinedLobby.Data[KEY_RELAY_CODE].Value == "")
                        return;

                    NextState();
                    return;

                case ELobbyState.JoiningRelay:
                    SetRequestInProgress();

                    await RelayHandler.Instance.JoinRelay(m_JoinedLobby.Data[KEY_RELAY_CODE].Value);
                    NextState();
                    return;

                case ELobbyState.WaitingGameManager:
                    while (!GameManager.FindInstance(true))
                        return;

                    NextState();
                    return;

                case ELobbyState.SendingPlayerData:
                    var playerData = m_PlayerData;                          // TODO : remove if no longer necessary
                    
                    GameManager.Instance.AddPlayerDataServerRPC(
                        NetworkManager.Singleton.LocalClientId,
                        StaticPlayerData.ToStruct()
                    );
                    NextState();
                    return;

                case ELobbyState.Ready:
                    return;


                default:
                    ErrorHandler.Error("Unhandled LobbyState : " + m_State);
                    return;
            }
        }

        /// <summary>
        /// Send Heartbeat to the lobby to maintain it alive
        /// </summary>
        async void HandleLobbyHeartbeat()
        {
            if (m_HostLobby == null)
                return;

            m_HeartbeatTimer -= Time.deltaTime;
            if (m_HeartbeatTimer > 0.0f)
                return;

            // reset heatbeat
            m_HeartbeatTimer = HEARTBEAT_TIMER;

            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(m_HostLobby.Id);

            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
                m_HeartbeatTimer = 0f;        // set timer back to 0 to send an other one
            }


        }

        async void UpdateLobbyData()
        {
            if (m_JoinedLobby == null)
                return;

            if (m_RequestInProgress)
                return;

            m_UpdateLobbyTimer -= Time.deltaTime;
            if (m_UpdateLobbyTimer > 0.0f)
                return;

            m_UpdateLobbyTimer = UPDATE_LOBBY_TIMER;

            SetRequestInProgress();

            m_JoinedLobby = await LobbyService.Instance.GetLobbyAsync(m_JoinedLobby.Id);

            SetRequestInProgress(false);
        }

        #endregion


        #region Join & Exit Lobby

        public async Task<bool> QuickJoinLobby()
        {
            bool success = await JoinLobby();

            if (!success)
                success = await CreateLobby();

            if (!success)
                Debug.Log("Failed to join or create lobby");

            SetState(ELobbyState.Joining);

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
                        Data = StaticPlayerData.ToPlayerDataObject()
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
                        Data = StaticPlayerData.ToPlayerDataObject()
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
        
        /// <summary>
        /// Leave the current joined lobby
        /// </summary>
        public async void LeaveLobby()
        {
            if (LobbyService.Instance != null && m_JoinedLobby != null)
                await LobbyService.Instance.RemovePlayerAsync(m_JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

            ResetLobby();

            ErrorHandler.Log("Lobby left", ELogTag.Lobby);
        }

        /// <summary>
        /// Delete lobby from Host
        /// </summary>
        async void DeleteHostLobby()
        {
            if (m_HostLobby == null)
                return;

            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(m_HostLobby.Id);
                m_HostLobby = null;
            }
            catch (LobbyServiceException e)
            {
                ErrorHandler.Error("Failed to delete lobby: " + e.Message);
            }
        }

        #endregion


        #region State Methods

        void SetState(ELobbyState state)
        {
            ErrorHandler.Log("NEW LOBBY STATE : " + state, ELogTag.Lobby);

            // if state changes : reset request in progress
            SetRequestInProgress(false);

            // set new state
            m_State = state;
        }

        void NextState()
        {
            SetState(m_State + 1);
        }

        void SetRequestInProgress(bool inProgress = true) 
        {
            // reset timers on reseting request in progress (to avoid spam the server)
            if (m_RequestInProgress && inProgress == false)
            {
                m_HeartbeatTimer = HEARTBEAT_TIMER;
                m_UpdateLobbyTimer = UPDATE_LOBBY_TIMER;
            }

            m_RequestInProgress = inProgress; 
        }

        #endregion


        #region Update Lobby Data Methods 

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

        async void UpdatePlayerData(string playerName = "", ECharacter character = ECharacter.Count)
        {
            var data = new Dictionary<string, PlayerDataObject>();
            if (playerName != "")
                data.Add(StaticPlayerData.KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName));
            if ( character != ECharacter.Count )
                data.Add(StaticPlayerData.KEY_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ((int)character).ToString()));

            try
            {
                await LobbyService.Instance.UpdatePlayerAsync(
                    m_JoinedLobby.Id, 
                    AuthenticationService.Instance.PlayerId, 
                    new UpdatePlayerOptions { Data = data }
                );
            } catch (LobbyServiceException e)
            {
                ErrorHandler.Error("Failed to update player name: " + e.Message);
            }
        }

        #endregion


        #region Dependent Accessors

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

        #endregion


        #region Tools Methods
        public async Task<List<Lobby>> ListLobbies()
        {
            try
            {
                QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
                {
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

                ErrorHandler.Log("Lobbies found: " + queryResponse.Results.Count, ELogTag.Lobby);

                foreach (var lobby in queryResponse.Results)
                {
                    ErrorHandler.Log("Lobby: " + lobby.Name + " - MaxPlayers : " + lobby.MaxPlayers, ELogTag.Lobby);
                }

                return queryResponse.Results;

            }
            catch (LobbyServiceException e)
            {
                ErrorHandler.Error("Failed to list lobbies: " + e.Message);
                return new List<Lobby>();
            }
        }

        /// <summary>
        /// Change Host of the Lobby
        /// </summary>
        async void MigradeLobbyHost()
        {
            try
            {
                m_HostLobby = await Lobbies.Instance.UpdateLobbyAsync(m_HostLobby.Id, new UpdateLobbyOptions
                {
                    HostId = m_JoinedLobby.Players[1].Id
                });
            }
            catch (LobbyServiceException e)
            {
                ErrorHandler.Error("Failed to migrate lobby host: " + e.Message);
            }
        }

        void PrintPlayers(Lobby lobby)
        {
            Debug.Log("Players in lobby : " + lobby.Id);
            foreach (var player in lobby.Players)
            {
                Debug.Log("Player: " + player.Id + " with name " + player.Data["PlayerName"].Value);
            }
        }

        #endregion


        #region Listeners

        void OnGameModeChanged(EGameMode gameMode)
        {
            Instance.GameMode = gameMode;
        }

        void OnArenaTypeChanged(EArenaType arenaType)
        {
            Instance.ArenaType = arenaType;
        }

        #endregion
    }
}