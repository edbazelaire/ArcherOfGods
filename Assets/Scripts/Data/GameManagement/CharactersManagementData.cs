using Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using UnityEngine;

namespace Data.GameManagement
{
    [Serializable]
    public struct SCharacterLevelData
    {
        /// <summary> required quantity of xp to level up </summary>
        public int RequiredXp;
        /// <summary> Golds gained on a level up </summary>
        public int BonusGolds;
        /// <summary> Golds gained on a level up </summary>
        public List<EChestType> BonusChests;

        public SCharacterLevelData(int xp, int bonusGolds, List<EChestType> bonusChests)
        {
            RequiredXp = xp;
            BonusGolds = bonusGolds;
            BonusChests = bonusChests;
        }
    }

    [CreateAssetMenu(fileName = "CharactersManagementData", menuName = "Game/Management/Characters")]
    public class CharactersManagementData : ScriptableObject
    {
        [Description("Stats bonus for each levels")]
        public float ScaleFactor = 1.1f;

        [Description("list of data for each levels (required xp to level up, bonus gained on level up, ...")]
        public List<SCharacterLevelData> CharacterLevelData;

        public SCharacterLevelData GetCharacterLevelData(int level)
        {
            if (level <= 0 || level > CharacterLevelData.Count)
            {
                ErrorHandler.Error("unable to find character level data for level " + level);
                return CharacterLevelData[0];
            }
            return CharacterLevelData[level - 1];
        }
    }
}