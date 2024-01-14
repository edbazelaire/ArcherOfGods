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
        public GameObject AoePrefab;
        public GameObject InstantSpellPrefab;
        public GameObject JumpPrefab;

        private SpellData[] m_SpellsList;
        private SpellData[] m_OnHitList;
        Dictionary<ESpell, SpellData> m_Spells { get; set; }
        Dictionary<string, SpellData> m_OnHitSpellData { get; set; }

        #endregion


        #region Initialization

        void Initialize()
        {
            InitializeSpells();

            DontDestroyOnLoad(s_Instance.gameObject);
        }

        void InitializeSpells()
        {
            LoadSpells();

            m_Spells = new Dictionary<ESpell, SpellData>();
            m_OnHitSpellData = new Dictionary<string, SpellData>();

            foreach (SpellData spell in m_SpellsList)
            {
                if (spell.name.StartsWith("_"))
                {
                    m_OnHitSpellData.Add(spell.name, spell);
                    continue;
                }

                if (spell.AnimationTimer < 0)
                    ErrorHandler.FatalError($"SpellLoader : AnimationTimer {spell.Spell} < 0");

                if (spell.CastAt < 0)
                    ErrorHandler.FatalError($"SpellLoader : Spell {spell.Spell} has a negative CastAt");

                if (spell.CastAt > 1)
                    ErrorHandler.FatalError($"SpellLoader : Spell {spell.Spell} has a CastAt > 1");

                if (spell.Cooldown <= 0)
                    spell.Cooldown = 0.1f;

                m_Spells.Add(spell.Spell, spell);
            }


            if (m_Spells.Count != (int)ESpell.Count)
            {
                ErrorHandler.FatalError("SpellLoader : Spell list is not complete");
                return;
            }
        }

        void LoadSpells()
        {
            m_SpellsList = Resources.LoadAll<SpellData>("Data/Spells");
            if (m_SpellsList == null)
            {
                ErrorHandler.FatalError("SpellLoader : No spells found");
                return;
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
                    if (s_Instance == null)
                        return null;
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
            if (!Instance.m_Spells.ContainsKey(spell))
            {
                ErrorHandler.Error($"SpellLoader : Spell {spell} not found");
                return null;
            }

            return Instance.m_Spells[spell];
        }

        /// <summary>
        /// Get the spell data of the given spell
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public static SpellData GetSpellData(string spellName)
        {
            if (Enum.TryParse(spellName, out ESpell spell))
            {
                return GetSpellData(spell);
            }

            if (!Instance.m_OnHitSpellData.ContainsKey(spellName))
            {
                ErrorHandler.Error($"SpellLoader : Spell {spellName} not found");
                return null;
            }

            return Instance.m_OnHitSpellData[spellName];
        }

        #endregion
    }

}
