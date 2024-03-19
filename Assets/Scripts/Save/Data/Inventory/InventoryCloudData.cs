using Assets;
using Enums;
using Game.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Unity.Services.CloudSave.Models;

namespace Save
{
    [Serializable]
    public struct SSpellCloudData
    {
        public ESpell Spell;
        public int Level;
        public int Qty;

        public SSpellCloudData(ESpell spell, int level, int qty)
        {
            Spell = spell;
            Level = level;
            Qty = qty;
        }
    }

    [Serializable]
    public struct SCharacterCloudData
    {
        public ECharacter Character;
        public int Level;
        public int Xp;

        public SCharacterCloudData(ECharacter character, int level = 1, int xp = 0)
        {
            Character = character;
            Level = level;
            Xp = xp;
        }
    }

    public class InventoryCloudData : CloudData
    {
        #region Members

        public new static InventoryCloudData Instance => Main.CloudSaveManager.GetCloudData(typeof(InventoryCloudData)) as InventoryCloudData;


        // ===============================================================================================
        // CONSTANTS
        // -- Values
        public ESpell[] DEFAULT_UNLOCKED_SPELLS => new ESpell[4] { ESpell.Arrow, ESpell.Heal, ESpell.Counter, ESpell.SmokeBomb };

        // -- Keys
        public const string KEY_GOLDS       = "Golds";
        public const string KEY_SPELL       = "Spell";
        public const string KEY_CHARACTERS  = "Characters";

        // ===============================================================================================
        // EVENTS
        /// <summary> action fired when the amount of gold changed </summary>
        public static Action<int> GoldChangedEvent;
        public static Action<SSpellCloudData> SpellDataChangedEvent;
        public static Action<SCharacterCloudData> CharacterDataChangedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { KEY_GOLDS, 0 },
            { KEY_CHARACTERS, new List<SCharacterCloudData>() },
            { KEY_SPELL, new List<SSpellCloudData>() },
        };

        #endregion


        #region Data Manipulators

        public override void SetData(string key, object value)
        {
            base.SetData(key, value);

            if (Enum.TryParse(key, out ERewardType reward) && reward == ERewardType.Golds)
            {
                GoldChangedEvent?.Invoke(System.Convert.ToInt32(value));
            }
                
        }

        public void SetData(ERewardType reward, object value)
        {
            SetData(reward.ToString(), value);
        }

        protected override object Convert(Item item)
        {
            if (m_Data[item.Key].GetType() == typeof(List<SSpellCloudData>))
                return item.Value.GetAs<SSpellCloudData[]>().ToList();

            if (m_Data[item.Key].GetType() == typeof(List<SCharacterCloudData>))
                return item.Value.GetAs<SCharacterCloudData[]>().ToList();

            return base.Convert(item);
        }

        #endregion


        #region Characters

        /// <summary>
        /// Get SSPellCloudData of a requested ESpell
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public SCharacterCloudData GetCharacter(ECharacter character)
        {
            int index = GetCharacterIndex(character);
            if (index == -1)
            {
                ErrorHandler.FatalError("Unable to find character " + character + " in character cloud data");
                return new SCharacterCloudData();
            }

            return ((List<SCharacterCloudData>)m_Data[KEY_CHARACTERS])[index];
        }

        /// <summary>
        /// Set the value of a SSpellCloudData
        /// </summary>
        /// <param name="characterCloudData"></param>
        public void SetCharacter(SCharacterCloudData characterCloudData)
        {
            int index = GetCharacterIndex(characterCloudData.Character);
            if (index >= 0)
                ((List<SCharacterCloudData>)m_Data[KEY_CHARACTERS])[index] = characterCloudData;
            else
                ((List<SCharacterCloudData>)m_Data[KEY_CHARACTERS]).Add(characterCloudData);

            CharacterDataChangedEvent?.Invoke(characterCloudData);
        }

        /// <summary>
        /// Get index of a spell in the list of SSpellCloudData
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public int GetCharacterIndex(ECharacter character)
        {
            var data = (List<SCharacterCloudData>)m_Data[KEY_CHARACTERS];
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Character != character)
                    continue;

                return i;
            }

            return -1;
        }

        /// <summary>
        /// Check if any spell is missing : if any, create it and save spells data
        /// </summary>
        /// <returns></returns>
        void CheckMissingCharacters()
        {
            foreach (ECharacter character in CharacterLoader.Instance.Characters.Keys)
            {
                if (character == ECharacter.Count)
                    continue;

                if (GetCharacterIndex(character) >= 0)
                    continue;

                // add new empty spell data, set save to false as we save the batch at the end
                SetCharacter(new SCharacterCloudData(character, 1, 0));
            }
        }

        #endregion


        #region Spells

        /// <summary>
        /// Get SSPellCloudData of a requested ESpell
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public SSpellCloudData GetSpell(ESpell spell)
        {
            if (SpellLoader.GetSpellData(spell).Linked)
            {
                var character = CharacterLoader.GetCharacterWithSpell(spell);
                return new SSpellCloudData(spell, GetCharacter(character.Value).Level, 0);
            }

            int index = GetSpellIndex(spell);
            if (index == -1)
            {
                ErrorHandler.FatalError("Unable to find spell " + spell + " in spell cloud data");
                return new SSpellCloudData();
            }

            return ((List<SSpellCloudData>)m_Data[KEY_SPELL])[index];
        }

        /// <summary>
        /// Set the value of a SSpellCloudData
        /// </summary>
        /// <param name="spellCloudData"></param>
        public void SetSpell(SSpellCloudData spellCloudData)
        {
            int index = GetSpellIndex(spellCloudData.Spell);
            if (index >= 0)
                ((List<SSpellCloudData>)m_Data[KEY_SPELL])[index] = spellCloudData;
            else
                ((List<SSpellCloudData>)m_Data[KEY_SPELL]).Add(spellCloudData);

            SpellDataChangedEvent?.Invoke(spellCloudData); 
        }

        /// <summary>
        /// Get index of a spell in the list of SSpellCloudData
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public int GetSpellIndex(ESpell spell)
        {
            var allSpells = (List<SSpellCloudData>)m_Data[KEY_SPELL];
            for (int i = 0; i < allSpells.Count; i++)
            {
                if (allSpells[i].Spell != spell)
                    continue;

                return i;
            }

            return -1;
        }

        /// <summary>
        /// Check if any spell is missing : if any, create it and save spells data
        /// </summary>
        /// <returns></returns>
        void CheckMissingSpells()
        {
            foreach (ESpell spell in SpellLoader.Spells)
            {
                // linked spell : level is dependent on the character
                if (SpellLoader.GetSpellData(spell).Linked)
                    continue;

                // already in data : skip
                if (GetSpellIndex(spell) >= 0)
                    continue;

                // check if should be unlocked (either a default spell or a character spell)
                int startLevel = DEFAULT_UNLOCKED_SPELLS.Contains(spell) ? 1 : 0;

                // add new empty spell data, set save to false as we save the batch at the end
                SetSpell(new SSpellCloudData(spell, startLevel, 0));
            }
        }

        #endregion


        #region Listeners

        protected override void OnCloudDataLoaded(string key)
        {
            base.OnCloudDataLoaded(key);

            // CHECKERS : check that data is conform to the expected data, add default data if some data is missing
            switch (key)
            {
                case KEY_CHARACTERS:
                    CheckMissingCharacters();
                    return;

                case KEY_SPELL:
                    CheckMissingSpells();
                    return;
            }
        }

        #endregion

    }
}