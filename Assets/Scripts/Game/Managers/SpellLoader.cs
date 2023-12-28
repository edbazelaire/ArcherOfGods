using Data;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Game.Managers
{
    public enum ESpells
    {
        Arrow,

        Count
    }

    public class SpellLoader : MonoBehaviour
    {
        #region Members

        public List<SpellData> SpellsList;

        static SpellLoader s_Instance;

        public Dictionary<ESpells, SpellData> Spells { get; set; }

        #endregion


        #region Initialization

        void Initialize()
        {
            Spells = new Dictionary<ESpells, SpellData>();  

            if (SpellsList.Count != (int)ESpells.Count)
            {
                ErrorHandler.FatalError("SpellLoader : Spell list is not complete");
                return;
            }

            foreach (SpellData spell in SpellsList)
                Spells.Add(spell.Name, spell);

            DontDestroyOnLoad(this);
        }

        #endregion


        #region Static Manipulators

        /// <summary>
        /// 
        /// </summary>
        public static SpellLoader Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<SpellLoader>();
                    s_Instance.Initialize();
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Get the spell data of the given spell
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public SpellData GetSpellData(ESpells spell)
        {
            if (!Spells.ContainsKey(spell))
            {
                ErrorHandler.Error($"SpellLoader : Spell {spell} not found");
                return null;
            }

            return Spells[spell];
        }

        #endregion
    }

}
