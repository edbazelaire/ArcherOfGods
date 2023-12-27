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

        public List<Controller> Controllers { get; set; }


        static GameManager s_Instance;
        List<Transform> m_Spawns;
        List<PlayerData> m_PlayerDatas;
        GameObject m_Arena;

        #endregion


        #region Inherited Manipulators

        private void Start()
        {
            Initialize();
        }

        #endregion


        #region Initialization 

        void Initialize()
        {
            // TODO : elsewhere
            m_PlayerDatas = new List<PlayerData>();
            ReceivePlayerData( new PlayerData(ECharacter.GreenArcher, true, 0) );
            ReceivePlayerData( new PlayerData(ECharacter.GreenArcher, false, 1) );
            /// ====================================================================
            
            InitializeArena();
            InitializeSpawns();
            CreatePlayers();
        }

        public void ReceivePlayerData(PlayerData playerData)
        {
            m_PlayerDatas.Add(playerData);
        }


        void InitializeArena()
        {
            m_Arena = GameObject.Find("Arena");
            if (!Checker.NotNull(m_Arena))
            {
                ErrorHandler.FatalError("Arena not found");
                return;
            }
        }   

        /// <summary>
        /// Initialize all spawns in the scene
        /// </summary>
        void InitializeSpawns()
        {
            m_Spawns = new List<Transform>();

            GameObject[] spawns = m_Arena.transform.FindGameObjectsWithTag("Respawn");

            foreach (GameObject spawn in spawns)
            {
                m_Spawns.Add(spawn.transform);
            }
        }

        /// <summary>
        /// Instantiate all players in the scene
        /// </summary>
        void CreatePlayers()
        {
            Controllers = new List<Controller>();

            for (int i = 0; i < m_PlayerDatas.Count; i++)
            {
                if (i >= m_Spawns.Count)
                {
                    ErrorHandler.Error("Not enough spawns for all players");
                    break;
                }

                // create requested character
                PlayerData playerData = m_PlayerDatas[i];
                GameObject character = CharacterLoader.Instance.InstantiateChar(playerData.Character, m_Spawns[i]);

                // get controller of the character
                Controller controller = character.GetComponent<Controller>();
                if (! Checker.NotNull(controller))
                    return;

                // init controller with a new health bar and add to list of controllers
                HealthBar healthBar = GameUIManager.Instance.CreateHealthBar(playerData.Team);
                if (!Checker.NotNull(healthBar))
                    return;

                controller.Initialize(healthBar);
                Controllers.Add(controller);
            }
        }

        #endregion


        #region Dependent Members

        public static GameManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new GameManager();
                    s_Instance.Initialize();
                }
                return s_Instance;
            }
        }

        #endregion
    }
}
