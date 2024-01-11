using Data;
using Enums;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Managers
{
    public class CharacterLoader: MonoBehaviour
    {
        #region Members

        static CharacterLoader s_Instance;

        public GameObject PlayerPrefab;


        [HideInInspector]
        public Dictionary<ECharacter, CharacterData> Characters;

        CharacterData[] m_CharactersList;

        #endregion


        #region Initialization

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Initialize()
        {
            LoadCharacterData();
        }

        void LoadCharacterData()
        {
            m_CharactersList = Resources.LoadAll<CharacterData>("Data/Characters");

            if (m_CharactersList.Length != (int)ECharacter.Count)
            {
                ErrorHandler.FatalError("CharacterLoader : number of characters is not equal to number of characters in enum");
                return;
            }

            Characters = new Dictionary<ECharacter, CharacterData>();
            foreach (CharacterData character in m_CharactersList)
            {
                if (Characters.ContainsKey(character.Name))
                {
                    ErrorHandler.FatalError($"CharacterLoader : Characters list contains duplicate : {character}");
                    return;
                }
                Characters.Add(character.Name, character);
            }
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
                    if (s_Instance == null)
                        return null;

                    s_Instance.Initialize();
                }
                return s_Instance;
            }
        }

        #endregion
    }

}
