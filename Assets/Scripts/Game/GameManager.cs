using Enums;
using Game.Managers;
using Network;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class GameManager: NetworkBehaviour
    {
        #region Members

        static GameManager s_Instance;

        // ===================================================================================
        // PRIVATE VARIABLES 
        // -- Network Variables
        NetworkVariable<EGameState>     m_State = new NetworkVariable<EGameState>(EGameState.LoadingScreen);
        NetworkList<int>                m_Players;

        // -- Data
        Dictionary<ulong, bool>         m_PlayersReady = new();
        Dictionary<ulong, ECharacter>   m_PlayersData = new();
        List<Controller>                m_Controllers;

        // ===================================================================================
        // PUBLIC ACCESSORS 
        public List<Controller>         Controllers                 => m_Controllers;

        #endregion



        #region Inherited Manipulators

        private void Awake()
        {
            m_Players = new ();
            m_Controllers = new List<Controller>();
        }

        private void Start()
        {
            s_Instance = this;

            m_State.OnValueChanged                              += OnStateValueChanged;
            m_Players.OnListChanged                             += OnPlayerListChanged;
            NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnected;
            LobbyHandler.Instance.OnRelayJoined                 += OnLobbyRelayJoind;

            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.Log("GameManager spawned");
        }

        #endregion


        #region Initialization 

        void Initialize()
        {
            return;
        }

        #endregion


        #region Private Manipulators

        void GameOver(int team)
        {
            Debug.Log($"Team {team} won");
        }

        void OnPlayerDied()
        {
            CheckWin();
        }

        void CheckPlayerDeath(int oldValue, int newValue)
        {
            if (newValue <= 0)
                OnPlayerDied();
        }

        void CheckWin()
        {
            var teamCtr = new List<int>();
            foreach (Controller controller in m_Controllers)
            {
                if (controller.Life.IsAlive && !teamCtr.Contains(controller.Team))
                    teamCtr.Add(controller.Team);
            }

            if (teamCtr.Count == 1)
            {
                GameOver(teamCtr[0]);
            }
        }

        #endregion


        #region Public Manipulators

        public Controller Owner
        {
            get
            {
                return GetPlayer(NetworkManager.Singleton.LocalClientId);
            }
        }

        void SpawnPlayers()
        {
            if (!IsServer)
                return;

            foreach (var player in m_Players)
            {
                ulong clientId = (ulong)player;
                var character = m_PlayersData[clientId];
                SpawnPlayer(clientId, character);
            }
        }

        void SpawnPlayer(ulong clientId, ECharacter character)
        {
            Debug.Log("SpawnPlayer");

            int team = m_Controllers.Count;

            // create player prefab and spawn it
            GameObject playerPrefab = Instantiate(CharacterLoader.Instance.PlayerPrefab);
            
            playerPrefab.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            // add player to list of player controllers
            Controller player = Finder.FindComponent<Controller>(playerPrefab);
            AddControllerClientRPC(clientId);

            // initialize player data
            player.Initialize(
                character: character,
                team: team
            );

            player.InitializeUIClientRPC((ECharacter)character, team);

            // add event listener to the player's hp
            player.Life.Hp.OnValueChanged += CheckPlayerDeath;
        }

        [ClientRpc]
        void AddControllerClientRPC(ulong clientId)
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                var controller = Finder.FindComponent<Controller>(player);
                if (controller.OwnerClientId == clientId)
                {
                    m_Controllers.Add(controller);
                    return;
                }
            }
        }

        [ClientRpc]
        public void ClientMessageClientRPC(string message)
        {
            Debug.Log(message);
        }
        
        public Controller GetPlayer(ulong clientId)
        {
            foreach (Controller controller in m_Controllers)
                if (controller.OwnerClientId == clientId)
                    return controller;

            return null;
        }

        public Controller GetFirstEnemy(int team)
        {
            foreach (Controller controller in m_Controllers)
                if (controller.Team != team)
                    return controller;

            return null;
        }
        
        public bool HasPlayer(ulong clientId)
        {
            return GetPlayer(clientId) != null;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddPlayerDataServerRPC(ulong clientId, ECharacter character)
        {
            Debug.Log("AddPlayerDataServerRPC + clientId " + clientId + " with character " + character.ToString());
            m_PlayersData.Add(clientId, character);

            // check if all players are there
            if (CheckGameStarted())
                m_State.Value = EGameState.InGame;
        }

        bool CheckGameStarted()
        {
            bool gameStarted =  m_Players.Count == LobbyHandler.Instance.MaxPlayers 
                && m_PlayersData.Count == m_Players.Count;

            return gameStarted;
        }

        #endregion


        #region Dependent Members

        public static GameManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindAnyObjectByType<GameManager>();
                    s_Instance.Initialize();
                }
                return s_Instance;
            }
        }

        #endregion

        #region Listeners

        void OnClientConnected(ulong clientId)
        {
            Debug.LogWarning("Client connected : " + clientId + " ==========================================================");

            if (!IsServer)
                return;

            m_Players.Add((int)clientId);
        }

        void OnPlayerListChanged(NetworkListEvent<int> changeEvent)
        {
            if (!IsServer)
                return;

            // check if all players are there
            if (CheckGameStarted())
                m_State.Value = EGameState.InGame;
        }

        void OnLobbyRelayJoind(ulong clientId, ECharacter character)
        {
            Debug.Log("OnLobbyRelayJoind");
            if (IsOwner)
                AddPlayerDataServerRPC(clientId, character);
        }

        void OnStateValueChanged(EGameState oldValue, EGameState newState)
        {
            Debug.Log("New state : " + newState);
            switch (newState) 
            {
                case EGameState.MainMenu:
                case EGameState.LoadingScreen:
                    break;

                case EGameState.InGame:
                    SpawnPlayers();
                    break;

            }
        }


        #endregion
    }
}
