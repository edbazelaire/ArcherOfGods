using Data;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Managers
{
    public enum ECharacter
    {
        GreenArcher,

        Count
    }

    public class CharacterLoader: MonoBehaviour
    {
        #region Members

        public List<CharacterData> CharactersList;
        public Dictionary<ECharacter, CharacterData> Characters;

        static CharacterLoader s_Instance;

        #endregion


        #region Initialization

        void Initialize()
        {
            Characters = new Dictionary<ECharacter, CharacterData>();
            if (CharactersList.Count != (int)ECharacter.Count)
            {
                ErrorHandler.FatalError("CharacterLoader : Characters list is not complete");
                return;
            }

            foreach (CharacterData character in CharactersList)
            {
                if (Characters.ContainsKey(character.Name))
                {
                    ErrorHandler.FatalError($"CharacterLoader : Characters list contains duplicate : {character}");
                    return;
                }
                Characters.Add(character.Name, character);
            }   

            DontDestroyOnLoad(this);
        }

        #endregion

        
        #region Public Static Manipulators

        public static CharacterData GetCharacterData(ECharacter character)
        {
            if (!CharacterLoader.Instance.Characters.ContainsKey(character))
            {
                ErrorHandler.FatalError($"CharacterLoader : Character {character} not found");
                return null;
            }

            return Instance.Characters[character];
        }

        #endregion


        #region Dependent Members

        public static CharacterLoader Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<CharacterLoader>();
                    s_Instance.Initialize();
                }
                return s_Instance;
            }
        }

        #endregion
    }

}
