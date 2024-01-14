using Game;
using Game.Spells;
using System.ComponentModel;
using Tools;
using UnityEngine;
using Enums;
using Unity.Netcode;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Unity.VisualScripting;
using Game.Managers;
using MyBox;
using Assets.Scripts.Game;
using System;

namespace Data
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Game/Spell")]
    public class SpellData : ScriptableObject
    {
        [Header("Identity")]
        [Description("Type of spell")]
        public ESpellType SpellType;

        [Header("UI")]
        [Description("Icon of the Spell")]
        public Sprite Image;
        [Description("Border of the Spell Card")]
        public Sprite Border;

        [Header("Prefabs")]
        [Description("Prefab of the spell that will be instantiated when the spell is cast")]
        public GameObject Graphics;
        [Description("Preview of the spell target on the ground displayed before the cast of the spell")]
        public GameObject Preview;
        [Description("Particles displayed during the animation")]
        public List<GameObject> AnimationPrefabs;
        [Description("Particles displayed when the cast is done")]
        public List<GameObject> OnCastPrefabs;
        [Description("Prefab of the spell when it hits a target")]
        public List<SpellData> OnHit;

        [Header("Stats")]
        [Description("Type of path that the spell is taking")]
        public ESpellTrajectory Trajectory;
        [Description("Maximum number of target that this spell can hit")]
        public int MaxHit = 1;
        [Description("Energy gained when this spell hits his target")]
        public int EnergyGain = 10;
        [Description("Request amount on energy to be able to cast this spell")]
        public int EnergyCost = 0;
        [Description("Damage of the spell")]
        public int Damage;
        [Description("Heals provided to the target")]
        public int Heal;
        [Description("Speed of the spell")]
        public float Speed;
        [Description("Max distance of the spell")]
        public float Distance = -1f;
        [Description("Duration of the aoe")]
        public float Duration;

        [Header("Collision")]
        [Description("Size of the spell (and hitbox)")]
        public float Size = 0.3f;
        [Description("Does the spell get trigger on touching a player")]
        public bool TriggerPlayer = true;

        [Header("State Effects")]
        [Description("List of effects that proc on hitting an enemy")]
        public List<SStateEffectData> EnemyStateEffects;
        [Description("List of effects that proc on hitting an ally")]
        public List<SStateEffectData> AllyStateEffects;

        //[ConditionalField("SpellType", false, ESpellType.Counter)]
        [Header("Counter")]
        public SCounterData CounterData;

        [Header("Animation & Cooldowns")]
        [Description("Name of the animation to use")]
        public EAnimation Animation;
        [Description("Time for the animation to take from start to begin (in seconds)")]
        public float AnimationTimer;
        [Description("Percentage of time during the animation when the spell will be casted")]
        public float CastAt = 1f;
        [Description("Cooldown to be able to re-use that ability")]
        public float Cooldown;

        public ESpell Spell 
        { 
            get
            {
                if (Enum.TryParse(name, out ESpell spell))
                    return spell;
                else
                    ErrorHandler.FatalError($"Spell {name} not found");

                return ESpell.Count;
            }
        }


        #region Public Manipualtors

        /// <summary>
        /// Cast a the spell by instantiating the prefab, initializing it and spawning in the network
        /// </summary>
        /// <param name="owner">            caster of the spell                         </param>
        /// <param name="targetPosition">   position targetted                          </param>
        /// <param name="position">         position where to spawn the spell prefab    </param>
        /// <param name="rotation">         rotation of the prefab                      </param>
        /// <returns></returns>
        public GameObject Cast(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default)
        {
            Debug.Log("Casting spell : " + name);
            GameObject spellGO;

            switch (SpellType)
            {
                case ESpellType.Projectile:
                    spellGO = GameObject.Instantiate(SpellLoader.Instance.ProjectilePrefab, position, rotation);
                    break;

                case ESpellType.Aoe:
                    spellGO = GameObject.Instantiate(SpellLoader.Instance.AoePrefab, position, rotation);
                    break;

                case ESpellType.Counter:
                case ESpellType.InstantSpell:
                    spellGO = GameObject.Instantiate(SpellLoader.Instance.InstantSpellPrefab, position, rotation);
                    break;
                
                case ESpellType.Jump:
                    spellGO = GameObject.Instantiate(SpellLoader.Instance.JumpPrefab, position, rotation);
                    break;

                default:
                    ErrorHandler.FatalError($"SpellType {SpellType} not implemented");
                    return null;
            }
            
            // spawn in network
            Finder.FindComponent<NetworkObject>(spellGO).SpawnWithOwnership(clientId);

            // initialize the spell
            var spell = Finder.FindComponent<Spell>(spellGO);
            spell.Initialize(target, name);

            // backpropagate the spell intialization to the client (for the preview)
            spell.InitializeClientRpc(target, name);

            return spellGO;
        }

        /// <summary>
        /// Spawn the prefabs that are displayed when the spell is casted
        /// </summary>
        /// <returns></returns>
        public List<GameObject> SpawnOnCastPrefabs(Transform parent, Vector3 target)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            // spawn on cast particles
            foreach (var prefab in OnCastPrefabs)
            {
                GameObject go = GameObject.Instantiate(prefab);
                Finder.FindComponent<OnCastAoe>(go).Initialize(Size);
                go.transform.position = target;
                gameObjects.Add(go);
            }

            return gameObjects;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SpawnOnHitPrefab(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default)
        {
            if (OnHit == null)
                return;

            foreach(SpellData spellData in OnHit)
            {
                spellData.Cast(clientId, target, position, rotation);
            }   
        }

        /// <summary>
        /// Display the preview of the spell on the ground where the player is aiming
        /// </summary>
        public void SpellPreview(Transform targettableArea, Transform parent, Vector3 offset = default)
        {
            if (Preview == null)
                return;

            switch (Trajectory)
            {
                case ESpellTrajectory.Curve:
                case ESpellTrajectory.Hight:
                    parent = ArenaManager.Instance.Arena.transform;
                    break;

                case ESpellTrajectory.Straight:
                    break;

                default:
                    Debug.LogError($"Trajectory {Trajectory} not implemented");
                    break;
            }

            // instantiate the gameobject of the preview
            var preview = GameObject.Instantiate(Preview, parent);
            // apply the offset
            preview.transform.localPosition += offset;

            // get the component of the preview and initialize it
            var component = Finder.FindComponent<SpellPreview>(preview);
            component.Initialize(targettableArea, Distance, Size);
        }

        #endregion
    }
}