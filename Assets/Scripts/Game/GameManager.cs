using Game.Managers;
using System.Collections.Generic;
using Tools;
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

    public class GameManager: MonoBehaviour
    {
        #region Members

        const string c_PlateformPrefix  = "Plateform_";
        const string c_SpawnerPrefix    = "SpawnPoint_";
        const int c_NumTeams = 2;
        const int c_PlayerTeam = 0;

        static GameManager      s_Instance;
        List<List<Transform>>   m_Spawns;
        List<PlayerData>        m_PlayerDatas;

        public List<Controller> Players;
        public Controller CurrentPlayer;
        public GameObject Arena;
        public Transform TargetHight;

        #endregion


        #region Inherited Manipulators

        private void Start()
        {
            s_Instance = Instance;
        }

        #endregion


        #region Initialization 

        void Initialize()
        {
            /// ====================================================================
            // TODO : elsewhere
            m_PlayerDatas = new List<PlayerData>();
            ReceivePlayerData( new PlayerData(ECharacter.GreenArcher, true, 0) );
            ReceivePlayerData( new PlayerData(ECharacter.GreenArcher, false, 1) );
            /// ====================================================================
            
            InitializeArena();
            InitializeSpawns();
            CreatePlayers();

            CurrentPlayer.SetupSpellUI();
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
            Arena = GameObject.Find("Arena");
            if (!Checker.NotNull(Arena))
            {
                ErrorHandler.FatalError("Arena not found");
                return;
            }

            TargetHight = Finder.Find(Arena, "TargetHight").GetComponent<Transform>();
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
        /// Instantiate all players in the scene
        /// </summary>
        void CreatePlayers()
        {
            Players = new List<Controller>();

            for (int i = 0; i < m_PlayerDatas.Count; i++)
            {
                int j = i % m_Spawns[m_PlayerDatas[i].Team].Count;

                // create requested character
                PlayerData playerData = m_PlayerDatas[i];
                bool isCurrentPlayer = playerData.Team == 0;
                Transform spawn = m_Spawns[playerData.Team][j];
                GameObject character = CharacterLoader.GetCharacterData(playerData.Character).Instantiate(
                    playerData.Team, 
                    playerData.IsPlayer, 
                    Arena.transform, 
                    spawn.position, 
                    spawn.rotation
                );
                
                Players.Add(character.GetComponent<Controller>());

                if (isCurrentPlayer)
                    CurrentPlayer = Players[i];
            }
        }

        #endregion


        #region Public Manipulators

        public Controller GetPlayer(int team)
        {
            foreach (Controller controller in Players)
                if (controller.Team == team)
                    return controller;

            return null;
        }

        public Controller GetEnemy(int myTeam)
        {
            foreach (Controller controller in Players)
                if (controller.Team != myTeam)
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
