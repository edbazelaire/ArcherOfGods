using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game
{
    public class ArenaManager : MonoBehaviour
    {

        #region Members

        static ArenaManager s_Instance;

        const string c_PlateformPrefix = "Plateform_";
        const string c_SpawnerPrefix = "SpawnPoint_";
        const string c_TargettableAreaPrefix = "TargettableArea_";
        const string c_Arena = "Arena";
        const string c_TargetHight = "TargetHight";
        const int c_NumTeams = 2;

        GameObject              m_Arena;
        Transform               m_TargetHight;
        List<Transform>         m_TargettableAreas;
        List<List<Transform>>   m_Spawns;

        public GameObject Arena => m_Arena;
        public List<List<Transform>> Spawns => m_Spawns;
        public Transform TargetHight => m_TargetHight;
        public Transform EnemyTargettableArea => GameManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).Team == 0 ? m_TargettableAreas[1] : m_TargettableAreas[0];
        public Transform AllyTargettableArea => GameManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).Team == 0 ? m_TargettableAreas[0] : m_TargettableAreas[1];

        #endregion


        #region Inherited Manipulators

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            Initialize();
        }

        #endregion


        #region Private Manipulators

        void Initialize()
        {
            InitializeArena();
            InitializeSpawns();
            InitializeTargetabbleArea();
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

            if (m_Spawns.Count < c_NumTeams)
                ErrorHandler.FatalError("Not enough spawns for each teams");

            Checker.CheckEmpty(m_Spawns);
        }

        /// <summary>
        /// Get all Targettable Areas ordered by id
        /// </summary>
        void InitializeTargetabbleArea()
        {
            m_TargettableAreas = new List<Transform>();

            // re-order targettable areas by id
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


        #region Static Accessors

        public static ArenaManager Instance
        {
            get
            {
                if (s_Instance != null)
                    return s_Instance;

                s_Instance = FindFirstObjectByType<ArenaManager>();

                if (s_Instance == null)
                    return null;

                s_Instance.Initialize();
                return s_Instance;
            }
        }

        #endregion

    }
}