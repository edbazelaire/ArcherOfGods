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

namespace Data
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Game/Spells/Default")]
    public class SpellData : ScriptableObject
    {
        [Header("Identity")]
        [Description("Type of spell")]
        public ESpellType SpellType;
        [Description("Type of spell")]
        public ERarety Rarety;

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
        [Description("Type of path that the spell is taking")]
        public ESpellTrajectory Trajectory;
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
        [Description("Speed of the spell")]
        public float                Speed           = 0f;
        [Description("Max distance of the spell")]
        public float                Distance        = -1f;
        [Description("Duration of the spell")]
        public float                Duration        = 0f;
        [Description("Delay of the spell to be instantiated after cast")]
        public float                Delay           = 0f;

        [Header("Collision")]
        [Description("Size of the spell (and hitbox)")]
        public float                Size = 0.3f;
        [Description("Does the spell get trigger on touching a player")]
        public bool                 TriggerPlayer = true;

        [Header("State Effects")]
        [Description("List of effects that proc on hitting an enemy")]
        public List<SStateEffectData> EnemyStateEffects;
        [Description("List of effects that proc on hitting an ally")]
        public List<SStateEffectData> AllyStateEffects;

        [Header("Counter")]
        [ConditionalField("SpellType", false, ESpellType.Counter)]
        public SCounterData         CounterData;

        [Header("Jump")]
        [ConditionalField("SpellType", false, ESpellType.Jump)]
        public EJumpType            JumpType        = EJumpType.None;

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

        public void Cast(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default)
        {
            // instantiate the prefab of the spell
            GameObject spellGO = GameObject.Instantiate(SpellLoader.GetSpellPrefab(name, SpellType), position, rotation);

            // spawn in network
            Finder.FindComponent<NetworkObject>(spellGO).SpawnWithOwnership(clientId);

            // initialize the spell
            var spell = Finder.FindComponent<Spell>(spellGO);
            spell.Initialize(target, name);

            // backpropagate the spell intialization to the client (for the preview)
            spell.InitializeClientRpc(target, name);
        }

        /// <summary>
        /// Spawn the prefabs that are displayed when the spell is casted
        /// </summary>
        /// <returns></returns>
        /// 
        public List<GameObject> SpawnOnCastPrefabs(Transform ownerTransform, Vector3 target)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            // spawn on cast particles
            foreach (var prefab in OnCastPrefabs)
            {
                GameObject go = GameObject.Instantiate(prefab);
                float delay = Delay;
                switch (SpellType)
                {
                    case ESpellType.Projectile:
                        if (Speed > 0)
                            delay += Math.Abs(target.x - ownerTransform.position.x) / Speed;
                        break;

                    default:
                        break;
                }

                Finder.FindComponent<OnCastAoe>(go).Initialize(target, Size, delay);
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
            if (OnHit == null)
                return;

            Debug.Log("SpellData.SpawnOnHitPrefab()");

            foreach(SpellData spellData in OnHit)
            {
                Debug.Log("Casting on hit prefab : " + spellData.name);
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