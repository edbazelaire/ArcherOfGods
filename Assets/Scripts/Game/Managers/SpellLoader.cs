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

        static SpellLoader s_Instance;

        public GameObject SpellPrefab;
        public GameObject ProjectilePrefab;
        public GameObject InstantSpellPrefab;

        private SpellData[] m_SpellsList;
        public Dictionary<ESpell, SpellData> Spells { get; set; }

        #endregion


        #region Initialization

        void Initialize()
        {
            InitializeSpells();

            DontDestroyOnLoad(this);
        }

        void InitializeSpells()
        {
            m_SpellsList = Resources.LoadAll<SpellData>("Data/Spells");

            Spells = new Dictionary<ESpell, SpellData>();

            if (m_SpellsList.Length != (int)ESpell.Count)
            {
                ErrorHandler.FatalError("SpellLoader : Spell list is not complete");
                return;
            }

            foreach (SpellData spell in m_SpellsList)
            {
                if (spell.AnimationTimer < 0)
                    ErrorHandler.FatalError($"SpellLoader : AnimationTimer {spell.Spell} < 0");

                if (spell.CastAt < 0)
                    ErrorHandler.FatalError($"SpellLoader : Spell {spell.Spell} has a negative CastAt");

                if (spell.CastAt > 1)
                    ErrorHandler.FatalError($"SpellLoader : Spell {spell.Spell} has a CastAt > 1");

                if (spell.Cooldown <= 0)
                    spell.Cooldown = 0.1f;

                Spells.Add(spell.Spell, spell);
            }
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
        public static SpellData GetSpellData(ESpell spell)
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
