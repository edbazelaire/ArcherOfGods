using Game;
using Game.Spells;
using System.ComponentModel;
using Tools;
using UnityEngine;
using Enums;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;
using Game.Managers;
using MyBox;
using System;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using Game.Character;
using System.Linq;
using Assets;

namespace Data
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Game/Spells/Default")]
    public class SpellData : ScriptableObject
    {
        [Description("Rarety of the spell")]
        public ERarety              Rarety;
        [Description("Is this spell linked to a specific character")]
        public bool                 Linked;

        [Header("Prefabs")]
        [Description("Prefab of the spell that will be instantiated when the spell is cast")]
        public GameObject           Graphics;
        [Description("Preview of the spell target on the ground displayed before the cast of the spell")]
        public GameObject           Preview;
        [Description("Particles displayed during the animation")]
        public List<SPrefabSpawn>   OnAnimation;
        [Description("Particles displayed when the cast is done")]
        public List<GameObject>     OnCastPrefabs;
        [Description("Prefab of the spell when it hits a target")]
        public List<SpellData>      OnHit;

        [Header("Stats")]
        [Description("Type of targetting for the spell")]
        public ESpellTarget         SpellTarget = ESpellTarget.EnemyZone;
        [Description("Maximum number of target that this spell can hit")]
        public int                  MaxHit          = 1;
        [Description("Energy gained when this spell hits his target")]
        public int                  EnergyGain      = 10;
        [Description("Request amount on energy to be able to cast this spell")]
        public int                  EnergyCost      = 0;
        [Description("Damage of the spell")]
        public int                  Damage          = 0;
        [Description("Heals provided to the target")]
        public int                  Heal            = 0;
        [Description("Max distance of the spell")]
        public float                Distance        = -1f;

        [Description("Duration of the spell")]
        public float                Duration        = 0f;
        [Description("Delay of the spell to be instantiated after cast")]
        public float                Delay           = 0f;

        [Header("Collision")]
        [Description("Size of the spell (and hitbox)")]
        [SerializeField] public float  m_Size = 1f;
        [Description("Does the spell get trigger on touching a player")]
        public bool                 TriggerPlayer = true;

        [Header("State Effects")]
        [Description("List of effects that proc on hitting an enemy")]
        public List<SStateEffectData> EnemyStateEffects;
        [Description("List of effects that proc on hitting an ally")]
        public List<SStateEffectData> AllyStateEffects;

        [Header("Animation & Cooldowns")]
        [Description("Name of the animation to use")]
        public EAnimation Animation;
        [Description("Can the animation be cancelled ?")]
        public bool IsCancellable = true;
        [Description("Time for the animation to take from start to begin (in seconds)")]
        public float AnimationTimer;
        [Description("Percentage of time during the animation when the spell will be casted")]
        public float CastAt = 1f;
        [Description("Cooldown to be able to re-use that ability")]
        public float Cooldown;

        public virtual ESpellType SpellType => ESpellType.InstantSpell;
        public float Size => m_Size * Main.SpellSizeFactor;

        private int m_Level = 1;
        public int Level => m_Level;

        public string SpellName
        {
            get
            {
                string spellName = name;
                if (spellName.EndsWith("(Clone)"))
                    spellName = spellName[..^"(Clone)".Length];

                return spellName;
            }
        }

        public ESpell Spell 
        { 
            get
            {
                if (Enum.TryParse(SpellName, out ESpell spell))
                    return spell;
                else
                    ErrorHandler.FatalError($"Spell {SpellName} not found");

                return ESpell.Count;
            }
        }


        #region Public Manipualtors

        /// <summary>
        /// Cast a the spell by instantiating the prefab, initializing it and spawning in the network
        /// </summary>
        /// <param name="clientId">     caster of the spell                         </param>
        /// <param name="target">       position targetted                          </param>
        /// <param name="position">     position where to spawn the spell prefab    </param>
        /// <param name="rotation">     rotation of the prefab                      </param>
        /// <returns></returns>
        public IEnumerator CastDelay(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default, float? delay = null)
        {
            if (delay == null)
                delay = Delay;

            while (delay > 0f)
            {
                delay -= Time.deltaTime;
                yield return null;
            }

            Cast(clientId, target, position, rotation);
        }

        public virtual void Cast(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default)
        {
            // recalculate target depending on spell type
            CalculateTarget(ref target, clientId);

            Debug.Log("Casting spell : " + SpellName);

            // instantiate the prefab of the spell
            GameObject spellGO = GameObject.Instantiate(GetSpellPrefab(), position, rotation);

            // spawn in network
            Finder.FindComponent<NetworkObject>(spellGO).SpawnWithOwnership(clientId);

            // initialize the spell
            var spell = Finder.FindComponent<Spell>(spellGO);
            spell.Initialize(target, SpellName, m_Level);

            // backpropagate the spell intialization to the client (for the preview)
            spell.InitializeClientRpc(target, SpellName, m_Level);
        }

        /// <summary>
        /// Spawn the prefabs that are displayed when the spell is casted
        /// </summary>
        /// <returns></returns>
        /// 
        public virtual List<GameObject> SpawnOnCastPrefabs(Transform ownerTransform, Vector3 target)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            // spawn on cast particles
            foreach (var prefab in OnCastPrefabs)
            {
                GameObject go = GameObject.Instantiate(prefab);
                Finder.FindComponent<OnCastAoe>(go).Initialize(target, Size, Delay);
                gameObjects.Add(go);
            }

            return gameObjects;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="position"></param>
        /// 
        /// <param name="rotation"></param>
        public void SpawnOnHitPrefab(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default)
        {
            if (OnHit == null || OnHit.Count == 0)
                return;

            foreach(SpellData spellData in OnHit)
            {
                // setup spell data to level of this spell
                spellData.Clone(m_Level).Cast(clientId, target, position, rotation);
            }   
        }

        /// <summary>
        /// Display the preview of the spell on the ground where the player is aiming
        /// </summary>
        public virtual void SpellPreview(Controller controller, Transform parent = default, Vector3 offset = default)
        {
            if (Preview == null)
                return;

            if (parent == default)
                parent = controller.SpellHandler.SpellSpawn;

            // instantiate the gameobject of the preview
            var preview = GameObject.Instantiate(Preview, parent);
            preview.transform.localPosition += offset;

            // get the component of the preview and initialize it
            var component = Finder.FindComponent<SpellPreview>(preview);
            component.Initialize(controller.SpellHandler.TargettableArea, Distance, Size);
        }

        #endregion


        #region Spell Helpers

        protected virtual GameObject GetSpellPrefab()
        {
            return SpellLoader.GetSpellPrefab(SpellName, SpellType);
        }

        void CalculateTarget(ref Vector3 target, ulong clientId)
        {
            if (!IsAutoTarget)
                return;

            switch (SpellTarget)
            {
                case ESpellTarget.Self:
                    target = GameManager.Instance.GetPlayer(clientId).transform.position;
                    return;

                case ESpellTarget.FirstAlly:
                    target = GameManager.Instance.GetFirstAlly(GameManager.Instance.GetPlayer(clientId).Team, clientId).transform.position;
                    return;

                case ESpellTarget.FirstEnemy:
                    target = GameManager.Instance.GetFirstEnemy(GameManager.Instance.GetPlayer(clientId).Team).transform.position;
                    return;

                default:
                    ErrorHandler.Error("Unhandled case : " + SpellTarget);
                    return;
            }
        }

        #endregion


        #region Dependent Members

        public bool IsAutoTarget
        {
            get
            {
                return SpellTarget == ESpellTarget.Self
                || SpellTarget == ESpellTarget.FirstAlly
                || SpellTarget == ESpellTarget.FirstEnemy;
            }
        }

        #endregion


        #region Infos Display

        public virtual Dictionary<string, object> GetInfos()
        {
            var infosDict = new Dictionary<string, object>();
            
            infosDict.Add("Type", GetTypeInfo());
            infosDict.Add("Energy", EnergyGain);

            if (Damage > 0)
                infosDict.Add("Damages", Damage);
            if (Heal > 0)
                infosDict.Add("Heal", Heal);
            if (Duration > 0)
                infosDict.Add("Duration", Duration);
            if (Size > 0)
                infosDict.Add("Size", m_Size);
            
            infosDict.Add("Cooldown", Cooldown);
            infosDict.Add("Cast", AnimationTimer * CastAt);

            if (Distance > 0)
                infosDict.Add("Distance", Distance);

            // add state effects
            AddStateEffectInfos(ref infosDict);
            
            // add on hit infos if has any
            AddOnHitInfos(ref infosDict);
            
            return infosDict;
        }

        /// <summary>
        /// Add on hit infos to infos dictionnary
        /// </summary>
        /// <param name="infosDict"></param>
        /// <returns></returns>
        void AddOnHitInfos(ref Dictionary<string, object> infosDict)
        {
            if (OnHit.Count == 0)
                return;

            if (OnHit.Count > 1)
            {
                ErrorHandler.Warning("OnHit.Count > 1 : this case is not handled in infos description");
                return;
            }

            string[] keysToIgnore = new string[]{ "Cooldown", "Cast", "Distance"};

            var onHitInfos = OnHit[0].GetInfos();
            foreach ( var info in onHitInfos )
            {
                // skip some keys
                if (keysToIgnore.Contains(info.Key))
                    continue;

                // do not override Jump type
                if (info.Key == "Type" && SpellType == ESpellType.Jump)
                    continue;

                infosDict[info.Key] = info.Value;
            }
        }

        void AddStateEffectInfos(ref Dictionary<string, object> infosDict)
        {
            if (AllyStateEffects.Count == 0 && EnemyStateEffects.Count == 0)
                return;

            if (! infosDict.ContainsKey("Effects"))
                infosDict.Add("Effects", new List<SStateEffectData>());
            
            foreach (var stateEffect in AllyStateEffects)
            {
                (infosDict["Effects"] as List<SStateEffectData>).Add(stateEffect);
            }
            foreach (var stateEffect in EnemyStateEffects)
            {
                (infosDict["Effects"] as List<SStateEffectData>).Add(stateEffect);
            }
        }

        public string GetTypeInfo()
        {
            return SpellType.ToString();
        }

        #endregion


        #region Level management

        public SpellData Clone(int level = 0)
        {
            var cloneSpellData = ScriptableObject.Instantiate(this);

            if (level > 0)
                cloneSpellData.SetLevel(level);

            return cloneSpellData;
        }

        public void SetLevel(int level)
        {
            // factor of current level
            float currentFactor = (float)Math.Pow(SpellLoader.SpellsManagementData.SpellLevelFactor, m_Level - 1);
            // factor of the level we are setting
            float newFactor = (float)Math.Pow(SpellLoader.SpellsManagementData.SpellLevelFactor, level - 1);
            
            // cooldown cant be below base cooldown
            if (Cooldown > 0)
                Cooldown    = Mathf.Max(Mathf.Round(100f * currentFactor * Cooldown / newFactor) / 100f, 0.5f);
            Damage      = (int)Math.Round(Damage    * newFactor / currentFactor );
            Heal        = (int)Math.Round(Heal      * newFactor / currentFactor);

            m_Level = level;
        }

        #endregion
    }
}