using Enums;
using Game.Managers;
using Game.Spells;
using Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Unity.Services.CloudSave.Models;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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

    public class InventoryCloudData : CloudData
    {
        #region Members

        // ===============================================================================================
        // KEYS
        public const string KEY_GOLDS = "Golds";
        public const string KEY_SPELL = "Spell";

        // ===============================================================================================
        // EVENTS
        /// <summary> action fired when the amount of gold changed </summary>
        public static Action<int> GoldChangedEvent;

        // ===============================================================================================
        // DATA
        /// <summary> default data for the Inventory </summary>
        protected override Dictionary<string, object> m_Data { get; set; } = new Dictionary<string, object>() {
            { KEY_GOLDS, 0 },
            { KEY_SPELL, new List<SSpellCloudData>() }
        };

        #endregion


        #region Loading & Saving

        public override void Load()
        {
            base.Load();

            CheckMissingSpells();
        }

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

            return base.Convert(item);
        }

        #endregion


        #region Spells

        public SSpellCloudData GetSpell(ESpell spell)
        {
            int index = GetSpellIndex(spell);
            if (index == -1)
            {
                ErrorHandler.FatalError("Unable to find spell " + spell + " in spell cloud data");
                return new SSpellCloudData();
            }

            return ((List<SSpellCloudData>)m_Data[KEY_SPELL])[index];
        }

        public void SetSpell(SSpellCloudData spellCloudData, bool save = true)
        {
            int index = GetSpellIndex(spellCloudData.Spell);
            if (index >= 0)
                ((List<SSpellCloudData>)m_Data[KEY_SPELL])[index] = spellCloudData;
            else
                ((List<SSpellCloudData>)m_Data[KEY_SPELL]).Add(spellCloudData);
        }

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
            bool save = false;
            foreach (ESpell spell in SpellLoader.Spells)
            {
                if (GetSpellIndex(spell) >= 0)
                    continue;

                // add new empty spell data, set save to false as we save the batch at the end
                SetSpell(new SSpellCloudData(spell, 0, 0), save: false);
                save = true;
            }

            // if any modifications : save new value
            if (save)
                SaveValue(KEY_SPELL);
        }

        public Dictionary<ESpell, SSpellCloudData> GetSpellCloudDataAsDict()
        {
            var dict = new Dictionary<ESpell, SSpellCloudData>();
            foreach (SSpellCloudData spellCloudData in (List<SSpellCloudData>)m_Data[KEY_SPELL])
            {
                if (dict.ContainsKey(spellCloudData.Spell))
                {
                    ErrorHandler.Error("Duplicated key spell : " + spellCloudData.Spell);
                    continue;
                }

                dict.Add(spellCloudData.Spell, spellCloudData);
            }

            return dict;
        }

        #endregion

    }
}