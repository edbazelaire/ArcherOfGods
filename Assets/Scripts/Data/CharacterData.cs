using Enums;
using Game.Managers;
using System;
using Tools;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("Spells")]
        public ESpell           AutoAttack;
        public ESpell           Ultimate;

        [Header("Stats")]
        public float            Size        = 1f;
        public float            Speed       = 1f;
        public float            AttackSpeed = 1f;
        public int              MaxHealth   = 100;
        public int              MaxEnergy   = 100;

        // -- private data
        private int m_Level = 1;
        public int Level => m_Level;

        public string CharacterName
        {
            get
            {
                string myName = name;
                if (myName.EndsWith("(Clone)"))
                    myName = myName[..^"(Clone)".Length];

                return myName;
            }
        }

        public ECharacter Character { 
            get 
            { 
                if (! Enum.TryParse(CharacterName, out ECharacter myCharacter))
                {
                    ErrorHandler.Error("Unable to parse character name : " + CharacterName);
                    return ECharacter.Count;
                }

                return myCharacter;
            } 
        }

        public GameObject InstantiateCharacterPreview(GameObject parent)
        {
            var go = GameObject.Instantiate(AssetLoader.LoadCharacterPreview(Character), parent.transform);
            return go;
        }

        public CharacterData Clone(int level = 0)
        {
            CharacterData clone = Instantiate(this);
            if (level == 0) 
                return clone;

            clone.SetLevel(level);
            return clone;
        }

        private void SetLevel(int level)
        {
            var currentFactor = Math.Pow(CharacterLoader.CharactersManagementData.ScaleFactor, m_Level - 1);
            var scaleFactor = Math.Pow(CharacterLoader.CharactersManagementData.ScaleFactor, level - 1);

            MaxHealth = (int)Math.Round(MaxHealth * scaleFactor / currentFactor);

            m_Level = level;
        }
    }
}