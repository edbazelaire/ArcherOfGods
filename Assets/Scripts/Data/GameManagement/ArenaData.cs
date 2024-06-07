﻿using Enums;
using Managers;
using Save;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Data.GameManagement
{
    [Serializable]
    public struct SStageData
    {
        public ECharacter Character;
        public int CharacterLevel;
    }

    [Serializable]
    public struct SArenaLevelData
    {
        public List<ESpell> Spells;
        public SRewardsData rewardsData;
        public List<SStageData> StageData;
    }


    [CreateAssetMenu(fileName = "ArenaData", menuName = "Game/Management/ArenaData")]
    public class ArenaData : ScriptableObject
    {
        #region Members

        public static Action<EArenaType, int> ArenaLevelCompletedEvent;

        [SerializeField] List<SArenaLevelData> m_ArenaLevelData;

        public int CurrentLevel => ProgressionCloudData.SoloArenas[ArenaType].CurrentLevel;
        public int CurrentStage => ProgressionCloudData.SoloArenas[ArenaType].CurrentStage;

        public EArenaType               ArenaType               => Enum.TryParse(name, out EArenaType arenaType) ? arenaType : EArenaType.FireArena;
        public List<SArenaLevelData>    ArenaLevelData          => m_ArenaLevelData;
        public SArenaLevelData          CurrentArenaLevelData   => GetArenaLevelData(CurrentLevel);
        public SStageData               CurrentStageData        => GetStageData(CurrentLevel, CurrentStage);
        public int                      MaxLevel                => m_ArenaLevelData.Count;

        #endregion


        #region Accessors

        /// <summary>
        /// Get data of the requested level
        /// </summary>
        /// <param name="arenaLevel"></param>
        /// <returns></returns>
        public SArenaLevelData GetArenaLevelData(int arenaLevel)
        {
            if (arenaLevel < 0 || arenaLevel >= m_ArenaLevelData.Count)
            {
                ErrorHandler.Error("Bad arena level : " + arenaLevel);
                arenaLevel = 0;
            }

            return m_ArenaLevelData[arenaLevel];
        }

        /// <summary>
        /// Get data of the requested stage in the requested level
        /// </summary>
        /// <param name="arenaLevel"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public SStageData GetStageData(int arenaLevel, int stage)
        {
            var stageDataList = GetArenaLevelData(arenaLevel).StageData;
            if (stage < 0 || stage >= stageDataList.Count)
            {
                ErrorHandler.Error("Bad arena level : " + arenaLevel);
                stage = 0;
            }

            return stageDataList[stage];
        }

        public void UpdateStageValue(bool up)
        {
            ProgressionCloudData.UpdateStageValue(ArenaType, up);
        }

        #endregion


        #region Player Data Manipulators

        /// <summary>
        /// Create PlayerData from AI ArenaData
        /// </summary>
        /// <returns></returns>
        public SPlayerData CreatePlayerData()
        {
            ERune rune;
            switch (ArenaType)
            {
                case EArenaType.FireArena:
                    rune = ERune.FireRune;
                    break;

                case EArenaType.FrostArena:
                    rune = ERune.FrostRune;
                    break;

                default:
                    rune = ERune.None;
                    break;
            }

            // set spell levels equal to character level
            List<int> spellLevels = new List<int>();
            for (int i = 0; i < CurrentArenaLevelData.Spells.Count; i++)
            {
                // make sure that character level is not > to max spell level
                spellLevels.Add(Math.Min(CurrentStageData.CharacterLevel, CollectablesManagementData.GetMaxLevel(ESpell.AxeThrow)));
            }

            // create & return PlayerData
            return new SPlayerData(
                playerName: CurrentStageData.Character.ToString(),
                characterLevel: CurrentStageData.CharacterLevel,
                character: CurrentStageData.Character,
                rune: rune,
                spells: CurrentArenaLevelData.Spells.ToArray(),
                spellLevels: spellLevels.ToArray(),
                profileData: CreateProfileData()
            );
        }

        /// <summary>
        /// Create ProfileData for the AI depending on character & progression
        /// </summary>
        /// <returns></returns>
        public SProfileDataNetwork CreateProfileData()
        {
            return new SProfileDataNetwork(
                gamerTag: CurrentStageData.Character.ToString(),
                avatar: CurrentStageData.Character.ToString(),
                border: GetBorder().ToString(),
                title: ETitle.None.ToString()
            );
        }

        /// <summary>
        /// Get Border of the "profile" depending on current level
        /// </summary>
        /// <returns></returns>
        public EBorder GetBorder()
        {
            if (CurrentLevel == 0)
                return EBorder.None;

            if (CurrentLevel == 1)
                return EBorder.LeagueBronze;

            if (CurrentLevel == 2)
                return EBorder.LeagueSilver;

            if (CurrentLevel == 3)
                return EBorder.LeagueGold;

            return EBorder.Rank1;
        }

        #endregion
    }
}