using Data;
using Enums;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Game.Managers
{
    public class SpellLoader : MonoBehaviour
    {
        #region Members

        public SpellData[] SpellsList;

        static SpellLoader s_Instance;

        public Dictionary<ESpells, SpellData> Spells { get; set; }

        #endregion


        #region Initialization

        void Initialize()
        {
            SpellsList = Resources.LoadAll<SpellData>("Data/Spells");

            Spells = new Dictionary<ESpells, SpellData>();  

            if (SpellsList.Length != (int)ESpells.Count)
            {
                ErrorHandler.FatalError("SpellLoader : Spell list is not complete");
                return;
            }

            foreach (SpellData spell in SpellsList)
            {
                if (spell.AnimationTimer <= 0)
                    ErrorHandler.FatalError($"SpellLoader : AnimationTimer {spell.Name} <= 0");

                if (spell.CastAt < 0)
                    ErrorHandler.FatalError($"SpellLoader : Spell {spell.Name} has a negative CastAt");

                if (spell.CastAt > 1)
                    ErrorHandler.FatalError($"SpellLoader : Spell {spell.Name} has a CastAt > 1");

                if (spell.Prefab == null)
                    ErrorHandler.FatalError($"SpellLoader : Prefab {spell.Name} not provided");

                if (spell.Cooldown <= 0)
                    spell.Cooldown = 0.1f;

                Spells.Add(spell.Name, spell);
            }

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
        public static SpellData GetSpellData(ESpells spell)
        {
            if (!Instance.Spells.ContainsKey(spell))
            {
                ErrorHandler.Error($"SpellLoader : Spell {spell} not found");
                return null;
            }

            return Instance.Spells[spell];
        }

        #endregion
    }

}
