﻿using Enums;
using Game.Loaders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using UnityEngine;

namespace Data.GameManagement
{
    [Serializable]
    public struct SLevelData
    {
        /// <summary> quantity of golds required to level up </summary>
        public int RequiredGolds;
        /// <summary> Number of cards required to level up </summary>
        public int RequiredQty;

        public SLevelData(int golds, int qty)
        {
            RequiredGolds = golds;
            RequiredQty = qty;
        }
    }

    [Serializable]
    public struct SRaretyData
    {
        public ERarety  Rarety;
        public Color    Color;
        public int      StartLevel;
    }

    [CreateAssetMenu(fileName = "CollectablesManagement", menuName = "Game/Management/Collectables")]
    public class CollectablesManagementData : ScriptableObject
    {
        #region Members

        [Description("Specific data for each rarety type of spells")]
        public List<SRaretyData> RaretyData;
        [Description("Quantity and Golds required for each character level up")]
        public List<SLevelData> CharacterLevelData;
        [Description("Quantity and Golds required for each level up")]
        public List<SLevelData> SpellLevelData;

        public static CollectablesManagementData s_Instance;

        public static CollectablesManagementData Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = AssetLoader.Load<CollectablesManagementData>("CollectablesManagementData", AssetLoader.c_ManagementDataPath);
                }

                return s_Instance;
            }
        }

        #endregion


        #region Data Management

        public static CollectableData GetData(Enum collectable, int level)
        {
            // load data of the item
            if (collectable.GetType() == typeof(ECharacter))
                return CharacterLoader.GetCharacterData((ECharacter)collectable, level);

            if (collectable.GetType() == typeof(ESpell))
                return SpellLoader.GetSpellData((ESpell)collectable, level);

            if (collectable.GetType() == typeof(ERune))
                return SpellLoader.GetRuneData((ERune)collectable, level);

            ErrorHandler.Error("Unable to find CollectionData for data " + collectable + " of type " + collectable.GetType());
            return null;
        }

        #endregion


        #region Type & Cast

        public static bool IsCollectableType(Type type)
        {
            foreach (ECollectableType collectableType in Enum.GetValues(typeof(ECollectableType)))
            {
                if (collectableType == ECollectableType.None)
                    continue;

                if (GetEnumType(collectableType) == type)
                    return true;
            }

            return false;
        }

        public static Type GetEnumType(ECollectableType collectableType)
        {
            switch (collectableType)
            {
                case ECollectableType.None:
                    ErrorHandler.Warning("None collectable type was provided");
                    return null;

                case ECollectableType.Spell:
                    return typeof(ESpell);

                case ECollectableType.Character:
                    return typeof(ECharacter);

                case ECollectableType.Rune:
                    return typeof(ERune);

                default:
                    ErrorHandler.Warning("Unhandled type provided : " + collectableType.ToString());
                    return null;
            }
        }

        public static bool TryGetCollectableType(Enum collectable, out ECollectableType collectableType, bool logError = false)
        {
            if (! Enum.TryParse(collectable.GetType().ToString().Split(".")[1][1..], out collectableType))
            {
                if (logError)
                    ErrorHandler.Error("unable to parse " + collectable.GetType().ToString() + " into ECollectableType");
                return false;
            }

            return true;
        }

        public static bool TryCast(string name, Type collectableType, out Enum collectable)
        {
            collectable = null;
            if (Enum.TryParse(collectableType, name, out object result))
            {
                collectable = (Enum)result;
                return true;
            }

            return false;
        }

        public static Enum Cast(string name, ECollectableType collectableType)
        {
            return Cast(name, GetEnumType(collectableType));
        }

        public static Enum Cast(string name, Type collectableType)
        {
            if (Enum.TryParse(collectableType, name, out object result))
                return (Enum)result;

            ErrorHandler.Error("Failed to parse enum value for value: " + name);
            return null;
        }

        #endregion


        #region Level & Rarety 

        public static int GetStartLevel(Enum collectable)
        {
            // start level of characters is always 1, indepedently of the rarity
            if (collectable.GetType() == typeof(ECharacter))
                return 1;
            
            return GetRaretyData(collectable).StartLevel;
        }

        public static int GetMaxLevel(Enum collectable)
        {
            // start level of characters is always 1, indepedently of the rarity
            if (collectable.GetType() == typeof(ECharacter))
                return Instance.CharacterLevelData.Count + 1;

            if (collectable.GetType() == typeof(ESpell) || collectable.GetType() == typeof(ERune))
                return Instance.SpellLevelData.Count + 1;

            ErrorHandler.Error("Unhandled type of level data for collectable " + collectable);
            return 1;
        }

        /// <summary>
        /// Get the data for a specific rarety
        /// </summary>
        /// <param name="rarety"></param>
        /// <returns></returns>
        public static SRaretyData GetRaretyData(ERarety rarety)
        {
            foreach (var raretyData in Instance.RaretyData)
            {
                if (raretyData.Rarety == rarety)
                    return raretyData;
            }

            ErrorHandler.Error("Unable to find rarety data for " + rarety);
            return Instance.RaretyData[0];
        }

        /// <summary>
        /// Get the data for a specific collectable (character, spell, rune, ...)
        /// </summary>
        /// <param name="collectable"> value of a collectable </param>
        /// <returns></returns>
        public static SRaretyData GetRaretyData(Enum collectable)
        {
            return GetRaretyData(GetData(collectable, 1).Rarety);
        }

        /// <summary>
        /// Get the Level Up data of the provided value (required golds, quantity, ...)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="rarety"></param>
        /// <returns></returns>
        public static SLevelData GetLevelData(Enum collectable, int level)
        {
            // error control
            if (level < 0 || level > GetMaxLevel(collectable))
                ErrorHandler.Warning("bad level provided : " + level);

            // no level yet - unlocked
            if (level <= 0)
                return new SLevelData(0, 1);

            // max level - return (0, 0)
            if (level >= GetMaxLevel(collectable))
                return new SLevelData(0, 0);

            // character has its own level up values (golds, qty, ...) and is not dependent on rarety
            if (collectable.GetType() == typeof(ECharacter))
                return GetCharacterLevelData(level);

            // Rune & Spells have same level up data
            if (collectable.GetType() == typeof(ESpell) || collectable.GetType() == typeof(ERune))
                return GetSpellLevelData(level, GetRaretyData(collectable).Rarety);

            ErrorHandler.Error("unable to find level data for " + collectable);
            return default;
        }

        /// <summary>
        /// Get the Character's Level Up data  
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static SLevelData GetCharacterLevelData(int level)
        {
            if (level <= 0)
            {
                ErrorHandler.Error("Provided level is <= 0 : " + level);
                level = 1;
            }

            if (level > Instance.CharacterLevelData.Count)
            {
                ErrorHandler.Error("CharacterLevelData requested for level " + level + " while max level is " + Instance.CharacterLevelData.Count + 1);
                return new SLevelData(0, 0);
            }

            return Instance.CharacterLevelData[level - 1];
        }

        /// <summary>
        /// Get the Spell Level Up data depending on the rarety and the level of the spell
        /// </summary>
        /// <param name="level"></param>
        /// <param name="rarety"></param>
        /// <returns></returns>
        public static SLevelData GetSpellLevelData(int level, ERarety? rarety = null)
        {
            int levelIndex = level - 1;
            if (rarety.HasValue)
            {
                SRaretyData raretyData = GetRaretyData(rarety.Value);
                levelIndex = level - raretyData.StartLevel;
            }

            if (levelIndex < 0 || levelIndex >= Instance.SpellLevelData.Count)
            {
                ErrorHandler.Error($"bad level index ({levelIndex}) for rarety ({rarety})");
                levelIndex = 0;
            }

            return new SLevelData(Instance.SpellLevelData[level - 1].RequiredGolds, Instance.SpellLevelData[levelIndex].RequiredQty);
        }

        #endregion

    }
}