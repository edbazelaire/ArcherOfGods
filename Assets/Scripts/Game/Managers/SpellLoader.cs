using Data;
using Enums;
using Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Game.Managers
{
    public class SpellLoader : MonoBehaviour
    {
        #region Members

        static SpellLoader s_Instance;

        Dictionary<string, GameObject> m_SpellsPrefabs;
        Dictionary<ESpell, SpellData> m_Spells;
        Dictionary<string, SpellData> m_OnHitSpellData;
        Dictionary<string, StateEffect> m_StateEffects;

        public static List<ESpell> Spells => Instance.m_Spells.Keys.ToList();

        #endregion


        #region Initialization

        void Initialize()
        {
            InitializeSpellPrefabs();
            InitializeSpells();
            InitializeStateEffects();

            DontDestroyOnLoad(s_Instance.gameObject);
        }

        void InitializeSpellPrefabs()
        {
            m_SpellsPrefabs = new Dictionary<string, GameObject>();

            GameObject[] spellPrefabs = AssetLoader.LoadSpellPrefabs();
            foreach (GameObject spellPrefab in spellPrefabs)
            {
                m_SpellsPrefabs.Add(spellPrefab.name, spellPrefab);
            }
        }

        void InitializeSpells()
        {
            SpellData[] spellList = LoadSpells();

            m_Spells = new Dictionary<ESpell, SpellData>();
            m_OnHitSpellData = new Dictionary<string, SpellData>();

            foreach (SpellData spell in spellList)
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
        }

        void InitializeStateEffects()
        {
            StateEffect[] allData = LoadStateEffects();

            m_StateEffects = new Dictionary<string, StateEffect>();

            foreach (StateEffect data in allData)
            {
                m_StateEffects.Add(data.name, data);
            }
        }

        SpellData[] LoadSpells()
        {
           return Resources.LoadAll<SpellData>("Data/Spells");
        }

        StateEffect[] LoadStateEffects()
        {
           return Resources.LoadAll<StateEffect>("Data/StateEffects");
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
        /// Get the specific prefab for a spell if exists, otherwise return default prefab for this type of spell
        /// </summary>
        /// <param name="spellName"></param>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public static GameObject GetSpellPrefab(string spellName, ESpellType spellType)
        {
            // check for specific prefab of the spell
            if (Instance.m_SpellsPrefabs.ContainsKey(spellName))
                return Instance.m_SpellsPrefabs[spellName];

            // conversion for inherited default spell types with no default prefabs
            if (spellType == ESpellType.Counter)
                spellType = ESpellType.InstantSpell;

            // check that exists
            if (! Instance.m_SpellsPrefabs.ContainsKey(spellType.ToString()))
                ErrorHandler.FatalError("Unable to find default prefab for spell type " + spellType.ToString());

            // returns default prefab for spell type
            return Instance.m_SpellsPrefabs[spellType.ToString()];
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

        /// <summary>
        /// Get all spells of a specific rarety
        /// </summary>
        /// <param name="rarety"></param>
        /// <returns></returns>
        public static List<SpellData> GetSpellsFromRarety(ERarety rarety) 
        { 
            List<SpellData> spells = new List<SpellData>();
            foreach (var spellData in Instance.m_Spells.Values)
            {
                if (spellData.Rarety != rarety)
                    continue;
                spells.Add(spellData);
            }

            return spells;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateEffect"></param>
        /// <returns></returns>
        public static StateEffect GetStateEffect(EStateEffect stateEffect)
        {
            string stateEffectName = stateEffect.ToString();

            if (! Instance.m_StateEffects.ContainsKey(stateEffectName))
            {
                StateEffect state = ScriptableObject.CreateInstance<StateEffect>();
                state.name = stateEffectName;
                return state;
            }

            return Instance.m_StateEffects[stateEffectName];
        }

        #endregion
    }

}
