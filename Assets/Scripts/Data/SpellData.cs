using Game;
using Game.Spells;
using System.ComponentModel;
using Tools;
using UnityEngine;
using Enums;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.VisualScripting;
using Game.Loaders;
using System;
using System.Collections;
using System.Linq;
using Data.GameManagement;
using System.Reflection;

namespace Data
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Game/Spells/Default")]
    public class SpellData : CollectionData
    {
        #region Members

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
        [SerializeField] public int     m_Damage          = 0;
        [Description("Heals provided to the target")]
        [SerializeField] public int     m_Heal            = 0;
        [Description("Max distance of the spell")]
        public float                Distance        = -1f;

        [Description("Duration of the spell")]
        [SerializeField] public float m_Duration = 0f;
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

        [Header("Graphics")]
        [Description("Delay for the spell visual to be deleted after end of the spell")]
        public float PersistanceAfterEnd = 0f;

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
        [SerializeField] protected float m_Cooldown;

        // ===========================================================================
        // Dependent Members
        public virtual ESpellType SpellType => ESpellType.InstantSpell;
        public float Size => m_Size * Settings.SpellSizeFactor;
        protected override Type m_EnumType => typeof(ESpell);
        public ESpell Spell => (ESpell)Id;

        // ===========================================================================
        // Level Dependent Members
        public virtual float LevelScaleFactor   => (float)Math.Pow(Settings.SpellScaleFactor, m_Level - 1);
        public virtual float Cooldown           => Mathf.Max(Mathf.Round(100f * m_Cooldown / LevelScaleFactor) / 100f, 0.5f);
        public virtual int Damage               => (int)Math.Round(m_Damage * LevelScaleFactor);
        public virtual int Heal                 => (int)Math.Round(m_Heal * LevelScaleFactor);
        public virtual float Duration           => m_Duration;

        #endregion


        #region Instantiate & Spawns

        /// <summary>
        /// Cast a the spell with a delay
        /// </summary>
        /// <param name="clientId">     caster of the spell                         </param>
        /// <param name="target">       position targetted                          </param>
        /// <param name="position">     position where to spawn the spell prefab    </param>
        /// <param name="rotation">     rotation of the prefab                      </param>
        /// <returns></returns>
        public IEnumerator CastDelay(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default, float? delay = null, bool recalculateTarget = true)
        {
            if (delay == null)
                delay = Delay;

            // recalculate target depending on spell type
            if (recalculateTarget)
                CalculateTarget(ref target, clientId);

            while (delay > 0f)
            {
                delay -= Time.deltaTime;
                yield return null;
            }

            Cast(clientId, target, position, rotation, recalculateTarget: false);
        }

        /// <summary>
        /// Cast a the spell by instantiating the prefab, initializing it and spawning in the network
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="recalculateTarget"></param>
        public virtual void Cast(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default, bool recalculateTarget = true)
        {
            ErrorHandler.Log("Casting spell : " + Name, ELogTag.Spells);

            if (recalculateTarget)
                CalculateTarget(ref target, clientId);

            // instantiate the prefab of the spell
            GameObject spellGO = GameObject.Instantiate(GetSpellPrefab(), position, rotation);

            // spawn in network
            Finder.FindComponent<NetworkObject>(spellGO).SpawnWithOwnership(clientId);

            // reparent if any
            Transform parent = FindParent(clientId);
            if (parent != null)
                spellGO.transform.SetParent(FindParent(clientId));

            // initialize the spell
            var spell = Finder.FindComponent<Spell>(spellGO);
            spell.Initialize(clientId, target, Name, m_Level);

            // backpropagate the spell intialization to the client (for the preview)
            spell.InitializeClientRpc(clientId, target, Name, m_Level);
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
                var onHitSpellData = spellData.Clone(m_Level);
                onHitSpellData.Override(this);
                onHitSpellData.Cast(clientId, target, position, rotation, false);
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


        #region Overriders 

        public void Override(SpellData overridingData)
        {
            // apply overrides
            if (overridingData.m_Damage > 0)
                m_Damage = overridingData.m_Damage;

            if (overridingData.m_Heal > 0)
                m_Heal = overridingData.m_Heal;
        }

        #endregion


        #region Spell Helpers

        protected virtual GameObject GetSpellPrefab()
        {
            return SpellLoader.GetSpellPrefab(Name, SpellType);
        }

        public void CalculateTarget(ref Vector3 target, ulong clientId) 
        {
            if (!IsAutoTarget)
                return;

            switch (SpellTarget)
            {
                case ESpellTarget.Self:
                    target = GameManager.Instance.GetPlayer(clientId).transform.position;
                    break;

                case ESpellTarget.FirstAlly:
                    target = GameManager.Instance.GetFirstAlly(GameManager.Instance.GetPlayer(clientId).Team, clientId).transform.position;
                    break;

                case ESpellTarget.FirstEnemy:
                    target = GameManager.Instance.GetFirstEnemy(GameManager.Instance.GetPlayer(clientId).Team).transform.position;
                    break;

                case ESpellTarget.AllyZoneCenter:
                case ESpellTarget.EnemyZoneCenter:
                    target = new Vector3(GameManager.Instance.GetPlayer(clientId).SpellHandler.TargettableArea.position.x, target.y, target.z);
                    break;

                case ESpellTarget.AllyZoneStart:
                case ESpellTarget.EnemyZoneStart:
                    var controller = GameManager.Instance.GetPlayer(clientId);
                    var centerPos = controller.SpellHandler.TargettableArea.position.x;

                    // direction usless ??
                    int direction = ArenaManager.GetAreaMovementDirection(controller.Team, SpellTarget == ESpellTarget.EnemyZoneStart);
                    target = new Vector3(centerPos - direction * ArenaManager.Instance.TargettableAreaSize / 2, target.y, target.z);
                    break;

                default:
                    ErrorHandler.Error("Unhandled case : " + SpellTarget);
                    break;
            }

            // clamp target between min/max xPos of the target zone
            var zoneCenter = GameManager.Instance.GetPlayer(clientId).SpellHandler.TargettableArea.position.x;
            target.x = Mathf.Clamp(target.x, zoneCenter - ArenaManager.Instance.TargettableAreaSize / 2, zoneCenter + ArenaManager.Instance.TargettableAreaSize / 2);
        }

        /// <summary>
        /// Method for children to change parent spawning if need be
        /// </summary>
        /// <returns></returns>
        protected virtual Transform FindParent(ulong clientId)
        {
            return null;
        }

        #endregion


        #region Properties by Reflection

        /// <summary>
        /// Get Reflection PropertyInfo of desire StateEffect property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected bool TryGetPropertyInfo(ESpellProperty property, out FieldInfo propertyInfo, bool throwError = true)
        {
            // Get the type of MyClass
            Type myType = this.GetType();

            // Get the PropertyInfo object for the provided property
            propertyInfo = myType.GetField(property.ToString(), BindingFlags.Public | BindingFlags.Instance);
           
            // check if the property exists
            if (propertyInfo == null)
            {
                if (throwError)
                    ErrorHandler.Error("Unknown property " + property + " for Spell " + name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the value of a property by Reflection
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected virtual void SetProperty(ESpellProperty property, object value)
        {
            if (!TryGetPropertyInfo(property, out FieldInfo propertyInfo))
                return;

            propertyInfo.SetValue(this, value);
        }

        /// <summary>
        /// Get the value of a property by Reflection
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual object GetProperty(ESpellProperty property)
        {
            if (!TryGetPropertyInfo(property, out FieldInfo propertyInfo))
                return null;

            return propertyInfo.GetValue(this);
        }

        public virtual float GetFloat(ESpellProperty property)
        {
            object value = GetProperty(property);
            if (value == null)
                return default;

            if (! float.TryParse(value.ToString(), out float result))
            {
                ErrorHandler.Error("Unable to parse in FLOAT value " + value + " of property " + property + " of spell " + name);
                return default;
            }

            return result;
        }

        public virtual int GetInt(ESpellProperty property)
        {
            object value = GetProperty(property);
            if (value == null)
                return default;

            if (! int.TryParse(value.ToString(), out int result))
            {
                ErrorHandler.Error("Unable to parse in FLOAT value " + value + " of property " + property + " of spell " + name);
                return default;
            }

            return result;
        }

        public virtual T GetProperty<T>(ESpellProperty property)
        {
            object value = GetProperty(property);
            if (value == null)
                return default;

            try
            {
                return (T)value;
            }
            catch (Exception ex)
            {
                ErrorHandler.Error(ex.Message);
                ErrorHandler.Error("Unable to parse value " + value + " of property " + property + " of spell " + name);
                return default;
            }
        }


        #endregion


        #region Infos Display

        public override Dictionary<string, object> GetInfos()
        {
            var infosDict = base.GetInfos();
            
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

        public new SpellData Clone(int level = 0)
        {
            return (SpellData)base.Clone(level);
        }

        #endregion


        #region Public Dependent Accessors

        public void ForceAutoTarget()
        {
            if (IsAutoTarget)
                return;

            switch (SpellTarget)
            {
                case (ESpellTarget.None):
                case (ESpellTarget.Free):
                case (ESpellTarget.EnemyZone):
                    SpellTarget = ESpellTarget.FirstEnemy;
                    break;

                case (ESpellTarget.AllyZone):
                    SpellTarget = ESpellTarget.FirstAlly;
                    break;
            }
        }

        public bool IsAutoTarget
        {
            get
            {
                return SpellTarget != ESpellTarget.None
                    && SpellTarget != ESpellTarget.EnemyZone
                    && SpellTarget != ESpellTarget.AllyZone
                    && SpellTarget != ESpellTarget.Free;
            }
        }

        public bool IsEnemyTarget => SpellTarget == ESpellTarget.FirstEnemy
            || SpellTarget == ESpellTarget.EnemyZone
            || SpellTarget == ESpellTarget.EnemyZoneStart
            || SpellTarget == ESpellTarget.EnemyZoneCenter;
            

        public bool IsAllyTarget => SpellTarget == ESpellTarget.FirstAlly
            || SpellTarget == ESpellTarget.Self
            || SpellTarget == ESpellTarget.AllyZone
            || SpellTarget == ESpellTarget.AllyZoneStart
            || SpellTarget == ESpellTarget.AllyZoneCenter;

        #endregion
    }
}