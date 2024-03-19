using Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using UnityEngine;

namespace Data.GameManagement
{
    [Serializable]
    public struct SSpellLevelData
    {
        /// <summary> quantity of golds required to level up </summary>
        public int RequiredGolds;
        /// <summary> Number of cards required to level up </summary>
        public int RequiredCards;

        public SSpellLevelData(int golds, int cards)
        {
            RequiredGolds = golds;
            RequiredCards = cards;
        }
    }

    [Serializable]
    public struct SRaretyData
    {
        public ERarety  Rarety;
        public Color    Color;
        public int      StartLevel;
    }

    [CreateAssetMenu(fileName = "SpellsManagement", menuName = "Game/Management/SpellsLevels")]
    public class SpellsManagementData : ScriptableObject
    {
        [Description("Percentage bonus of stats based on level")]
        public float SpellLevelFactor = 1.1f;

        [Description("Specific data for each rarety type of spells")]
        public List<SRaretyData> RaretyData;
        [Description("Cards and golds required for each level up")]
        public List<SSpellLevelData> SpellLevelData;

        public int MaxLevel => SpellLevelData.Count;


        #region Public Accessors

        /// <summary>
        /// Get the data for a specific spell rarety
        /// </summary>
        /// <param name="rarety"></param>
        /// <returns></returns>
        public SRaretyData GetRaretyData(ERarety rarety)
        {
            foreach (var raretyData in RaretyData)
            {
                if (raretyData.Rarety == rarety)
                    return raretyData;
            }

            ErrorHandler.FatalError("Unable to find rarety data for " + rarety);
            return RaretyData[0];
        }

        /// <summary>
        /// Get the Spell Level Up data depending on the rarety and the level of the spell
        /// </summary>
        /// <param name="level"></param>
        /// <param name="rarety"></param>
        /// <returns></returns>
        public SSpellLevelData GetSpellLevelData(int level, ERarety rarety)
        {
            SRaretyData raretyData = GetRaretyData(rarety);
            int levelIndex = level - raretyData.StartLevel;
            if (levelIndex < 0)
            {
                ErrorHandler.Error($"provided level ({level}) is inferior to start level ({raretyData.StartLevel}) for rarety ({rarety})");
                levelIndex = 0;
            }

            return new SSpellLevelData(SpellLevelData[level].RequiredGolds, SpellLevelData[levelIndex].RequiredCards);
        }

        #endregion

    }
}