using Enums;
using Game.Managers;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class PlayerData
    {
        public ECharacter   Character;
        public bool         IsPlayer;
        public int          Team;

        public PlayerData(ECharacter character, bool isPlayer, int team)
        {
            Character = character;
            IsPlayer = isPlayer;
            Team = team;
        }
    }

    public class GameManager: NetworkBehaviour
    {
        #region Members

        const string            c_PlateformPrefix           = "Plateform_";
        const string            c_SpawnerPrefix             = "SpawnPoint_";
        const string            c_TargettableAreaPrefix     = "TargettableArea_";
        const string            c_Arena                     = "Arena";
        const string            c_TargetHight               = "TargetHight";
        const int               c_NumTeams                  = 2;
        const int               c_PlayerTeam                = 0;

        static GameManager      s_Instance;
        List<List<Transform>>   m_Spawns;
        List<PlayerData>        m_PlayerDatas;

        public GameObject       m_Arena;
        public Transform        m_TargetHight;
        public List<Transform>  m_TargettableAreas;

        public List<Controller> Players;
        public Controller       CurrentPlayer;

        public GameObject               Arena               => m_Arena;
        public List<List<Transform>>    Spawns              => m_Spawns;
        public Transform                TargetHight         => m_TargetHight;
        public List<Transform>          TargettableAreas    => m_TargettableAreas;

        #endregion


        #region Inherited Manipulators


        #endregion


        #region Initialization 

        void Initialize()
        {
            InitializeArena();
            InitializeSpawns();
            InitializeTargetabbleArea();
        }

        public void ReceivePlayerData(PlayerData playerData)
        {
            m_PlayerDatas.Add(playerData);
        }


        /// <summary>
        /// 
        /// </summary>
        void InitializeArena()
        {
            m_Arena = GameObject.Find(c_Arena);
            if (!Checker.NotNull(Arena))
            {
                ErrorHandler.FatalError("Arena not found");
                return;
            }

            m_TargetHight = Finder.FindComponent<Transform>(m_Arena, c_TargetHight);
        }   

        /// <summary>
        /// Initialize all spawns in the scene
        /// </summary>
        void InitializeSpawns()
        {
            m_Spawns = new List<List<Transform>>();

            List<GameObject> plateforms = Finder.Finds(Arena, c_PlateformPrefix);
            int team = 0;
            foreach (GameObject plateform in plateforms)
            {
                List<Transform> spawns = new List<Transform>();
                foreach (GameObject spawner in Finder.Finds(plateform, c_SpawnerPrefix))
                    spawns.Add(spawner.transform);

                m_Spawns.Add(spawns);
                team++;
            }

            Checker.CheckEmpty(m_Spawns);
        }

        /// <summary>
        /// Get all Targettable Areas ordered by id
        /// </summary>
        void InitializeTargetabbleArea()
        {
            m_TargettableAreas = new List<Transform>();

            int i = 0;
            bool end = false;
            while (!end)
            {
                // set end to true by default
                end = true;

                foreach (Transform area in Finder.FindComponents<Transform>(Arena, c_TargettableAreaPrefix))
                {
                    // if a targettable area with this id is found
                    if (area.gameObject.name.EndsWith(i.ToString()))
                    {
                        // add it to the list
                        m_TargettableAreas.Add(area);
                        // continue to search
                        end = false;
                        break;
                    }
                }

                // increment id of target area to search
                i++;
            }
            
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
            foreach (Controller controller in Players)
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

        public void AddPlayer(Controller player)
        {
            player.Life.Hp.OnValueChanged += CheckPlayerDeath;
            Players.Add(player);
        }

        public Controller GetPlayer(ulong clientId)
        {
            foreach (Controller controller in Players)
                if (controller.OwnerClientId == clientId)
                    return controller;

            return null;
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
    }
}
