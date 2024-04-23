﻿using System;
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

        float                   m_TargettableAreaSize;

        public GameObject               Arena                   => m_Arena;
        public List<List<Transform>>    Spawns                  => m_Spawns;
        public Transform                TargetHight             => m_TargetHight;
        public float                    TargettableAreaSize     => m_TargettableAreaSize;

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

            m_TargettableAreaSize = m_TargettableAreas[0].GetComponent<RectTransform>().rect.width;
        }

        #endregion


        #region Static Accessors

        /// <summary>
        /// Depending on the team, the enemy and ally areas are inverted
        /// </summary>
        /// <param name="team"></param>
        /// <param name="enemyArea"></param>
        /// <returns></returns>
        public static Transform GetTargettableArea(int team, bool enemyArea = true)
        {
            return (team == 0 && enemyArea || team == 1 && ! enemyArea) ? Instance.m_TargettableAreas[1] : Instance.m_TargettableAreas[0];
        }

        /// <summary>
        /// Depending on which area is selected (ally or enemy) and the team of the player, the "movement direction" (= what is consider to be "forward")
        /// of a spell will not be the same.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="enemyArea"></param>
        /// <returns></returns>
        public static int GetAreaMovementDirection(int team, bool enemyArea = true)
        {
            return (team == 0 && enemyArea || team == 1 && !enemyArea) ? 1 : -1;
        }

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