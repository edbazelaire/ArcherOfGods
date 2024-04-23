using Enums;
using Game.Spells;
using Save;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Structural data that can be provided to create a new character 
    /// </summary>
    [Serializable]
    public struct SPlayerData : INetworkSerializable
    {
        public string       PlayerName;
        public int          CharacterLevel;
        public ECharacter   Character;
        public ERune        Rune;
        public ESpell[]     Spells;
        public int[]        SpellLevels;

        public SPlayerData(string playerName, int characterLevel, ECharacter character, ERune rune, ESpell[] spells, int[] spellLevels)
        {
            PlayerName = playerName;
            CharacterLevel = characterLevel;
            Character = character;
            Rune = rune;
            Spells = spells;
            SpellLevels = spellLevels;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref CharacterLevel);
            serializer.SerializeValue(ref Character);
            serializer.SerializeValue(ref Rune);
            serializer.SerializeValue(ref Spells);
            serializer.SerializeValue(ref SpellLevels);
        }
    }

    /// <summary>
    /// Static class containing access to all data necessary to start a game and can be converted in any necessary types 
    ///     + SPlayerData :                     for the GameManager
    ///     + Dict<string, PlayerDataObject> :  for the Lobby
    /// </summary>
    public static class StaticPlayerData
    {
        public const string KEY_PLAYER_NAME         = "PlayerName";
        public const string KEY_CHARACTER_LEVEL     = "CharacterLevel";
        public const string KEY_CHARACTER           = "Character";
        public const string KEY_RUNE                = "Rune";
        public const string KEY_SPELLS              = "Spells";
        public const string KEY_SPELL_LEVELS        = "SpellLevels";

        public static string        PlayerName      => ProfileCloudData.GamerTag;
        public static int           CharacterLevel  => InventoryCloudData.Instance.GetCollectable(Character).Level;
        public static ECharacter    Character       => CharacterBuildsCloudData.SelectedCharacter;
        public static ERune         Rune            => CharacterBuildsCloudData.CurrentRune;
        public static ESpell[]      Spells          => CharacterBuildsCloudData.CurrentBuild;
        public static int[]         SpellLevels
        {
            get
            {
                int[] spellLevels = new int[Spells.Length];
                for (int i=0; i < Spells.Length; i++)
                {
                    spellLevels[i] = InventoryCloudData.Instance.GetSpell(Spells[i]).Level;
                }

                return spellLevels;
            }
        }


        #region Data Conversion

        /// <summary>
        /// Convert it's data into a struct obj readable by the GameManager
        /// </summary>
        /// <returns></returns>
        public static SPlayerData ToStruct()
        {
            return new SPlayerData(PlayerName, CharacterLevel, Character, Rune, Spells, SpellLevels);
        }

        /// <summary>
        /// Convert it's data into data readable by the Lobby
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, PlayerDataObject> ToPlayerDataObject()
        {
            return new Dictionary<string, PlayerDataObject> {
                { KEY_PLAYER_NAME,          new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, StaticPlayerData.PlayerName) },
                { KEY_CHARACTER,            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ((int)Character).ToString()) },
                { KEY_RUNE,                 new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ((int)Rune).ToString()) },
                { KEY_SPELLS,               new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Spells.ToString()) },
                { KEY_SPELL_LEVELS,         new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, SpellLevels.ToString()) },
            };
        }

        #endregion

        #region Debug

        public static void Display()
        {
            Debug.LogWarning("PlayerData =================================================");
            Debug.Log("     + " + KEY_PLAYER_NAME + " : " + PlayerName);
            Debug.Log("     + " + KEY_CHARACTER + " : " + Character.ToString());
        }

        #endregion
    }
}