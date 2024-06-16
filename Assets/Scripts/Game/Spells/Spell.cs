﻿using Assets.Scripts.Managers.Sound;
using Data;
using Enums;
using Game.Loaders;
using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class Spell : NetworkBehaviour
    {
        #region Members

        // ========================================================================================================
        // Constants
        const string c_GraphicsContainer = "GraphicsContainer";

        // ========================================================================================================
        // Data
        protected SpellData m_BaseSpellData;
        SpellData m_SpellData => m_BaseSpellData;
        protected Controller        m_Controller;
        protected Vector3           m_Target;
        protected GameObject        m_GraphicsContainer;

        /// <summary> in case of persistance of graphisme, allows to stop spell behavior </summary>
        protected bool              m_IsOver = false;   
        /// <summary> timer delaying the end of spell graphismes after end of spell </summary>
        protected float             m_PersistanceTimer;
        /// <summary> counter of remaining number of target that this spell can hit </summary>
        protected List<ulong>       m_HittedPlayerId;
        /// <summary> on cast prefabs (spawn on cast) </summary>
        protected List<GameObject>  m_OnCastPrefabs;

        // ========================================================================================================
        // Events
        public Action OnSpellEndedEvent;

        // ========================================================================================================
        // Public Accessors
        public SpellData SpellData => m_SpellData;
        public Controller Controller => m_Controller;

        #endregion


        #region Init & End

        protected virtual void SetSpellData(string spellName, int level)
        {
            m_BaseSpellData = SpellLoader.GetSpellData(spellName, level);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellName"></param>
        public virtual void Initialize(ulong clientId, Vector3 target, string spellName, int level)
        {
            m_Controller        = GameManager.Instance.GetPlayer(clientId);
            SetSpellData(spellName, level);
            m_HittedPlayerId    = new List<ulong>();

            // set the target
            SetTarget(target);

            // initialize graphics of the spell (whith delay if has any)
            InitGraphics();
        }

        /// <summary>
        /// Instantiate the graphics of the spell
        /// </summary>
        protected virtual void InitGraphics()
        {
            m_GraphicsContainer = Finder.Find(gameObject, c_GraphicsContainer, throwError: false);
            if (m_GraphicsContainer == null)
                m_GraphicsContainer = new GameObject(c_GraphicsContainer);

            if (m_SpellData.Graphics != null)
            {
                ErrorHandler.Log("InitGraphics() : " + m_SpellData.Graphics + " with size " + m_SpellData.Size, ELogTag.Spells);
                Instantiate(m_SpellData.Graphics, m_GraphicsContainer.transform);
            }

            transform.localScale = new Vector3(m_SpellData.Size, m_SpellData.Size, 1f);

            if (m_SpellData.PermanantSoundFX != null)
                SoundFXManager.PlaySoundFXClip(m_SpellData.PermanantSoundFX, transform);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void End()
        {
            if (m_IsOver)
                return;

            // set is over to true (in case of persistance of graphisme, to stop spell behavior)
            m_IsOver = true;

            // visual ending effect
            SpawnOnHitPrefab();

            // play sound effect
            if (m_SpellData.OnEndSoundFX != null)
                GameManager.Instance.PlaySoundClientRPC(m_SpellData.Name, ESpellActionPart.OnEnd);

            // destroy the spell game object
            StartCoroutine(DestroySpell());

            ErrorHandler.Log("End of spell : " + m_SpellData, ELogTag.Spells);
        }

        public virtual IEnumerator DestroySpell()
        {
            // delay destruction of spell graphismes for visual purpuses
            m_PersistanceTimer = m_SpellData.PersistanceAfterEnd;

            while (m_PersistanceTimer > 0) 
            {
                m_PersistanceTimer -= Time.deltaTime;
                yield return null;
            }

            // call an end on client side
            EndClientRpc();

            // destroy the spell
            Destroy(gameObject);
        }

        #endregion


        #region Client RPCs

        /// <summary>
        /// Client is initialized with same data as server to create a previsualisation of the spell
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellType"></param>
        [ClientRpc]
        public void InitializeClientRpc(ulong clientId, Vector3 target, string spellName, int level)
        {
            if (IsHost)
                return;
            Initialize(clientId, target, spellName, level);
        }

        [ClientRpc]
        public void EndClientRpc()
        {
            OnSpellEndedEvent?.Invoke();
        }

        #endregion


        #region Inherited Manipulators  

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Update()
        {
            if (m_IsOver)
                return;

            UpdateMovement();
        }

        #endregion


        #region Protected Manipulators

        /// <summary>
        /// Update the position of the spell and [SERVER] check if the spell has reached its max distance
        /// </summary>
        protected virtual void UpdateMovement() { }

        /// <summary>
        /// Check if a Player has been hit
        /// </summary>
        /// <param name="controller"></param>
        protected virtual void OnHitPlayer(Controller controller)
        {
            // not alive : skip
            if (!controller.Life.IsAlive)
                return;

            // already hit
            if (m_HittedPlayerId.Contains(controller.OwnerClientId))
                return;

            // apply effects on ally or enemy : if none, skip
            if (! CheckHitEnemy(controller) && ! CheckHitAlly(controller))
                return;

            // add plyer id to list of hitted players
            m_HittedPlayerId.Add(controller.OwnerClientId);

            // energy gain
            m_Controller.EnergyHandler.AddEnergy(m_SpellData.EnergyGain);

            // play sound effect
            GameManager.Instance.PlaySoundClientRPC(m_SpellData.Name, ESpellActionPart.OnHit);

            // update hit count
            if (m_HittedPlayerId.Count <= m_SpellData.MaxHit && m_SpellData.MaxHit > 0)
                End();
        }

        /// <summary>
        /// Check if ability hit an enemy
        /// </summary>
        /// <param name="controller"> controller of hit target </param>
        /// <returns></returns>
        protected virtual bool CheckHitEnemy(Controller controller)
        {
            // Target is Ally - return
            if (controller.Team == m_Controller.Team)
                return false;

            // no base Damages, StateEffects or OnHit effects - return
            if (m_SpellData.Damage <= 0 && m_SpellData.EnemyStateEffects.Count == 0 && m_SpellData.OnHit.Count == 0)
                return false;

            // add bonus damages from state bonus & boosts 
            int damages = m_Controller.StateHandler.ApplyBonusDamages(m_SpellData.Damage);

            // check if target has counter(s)
            if (controller.CounterHandler.CheckCounters(this))
                return false;
            
            // get final damages after shields and resistances
            int finalDamages = controller.Life.Hit(damages);
            if (finalDamages > 0 && m_Controller.ClientAnalytics != null)
                m_Controller.ClientAnalytics.SendSpellDataClientRPC(m_SpellData.Name, EHitType.Damage, finalDamages);

            ErrorHandler.Log(m_SpellData.Name + " : " + finalDamages, ELogTag.Spells);

            // apply lifesteal if any (remove 1 because floats values are always based on 1 as default value)
            float lifeSteal = Mathf.Max(0f, m_Controller.StateHandler.GetFloat(EStateEffectProperty.LifeSteal) - 1);
            if (lifeSteal > 0 && finalDamages > 0)
            {
                m_Controller.Life.Heal((int)Mathf.Round(lifeSteal * finalDamages));

                if (m_Controller.ClientAnalytics != null)
                {
                    m_Controller.ClientAnalytics.SendSpellDataClientRPC(m_SpellData.Name, EHitType.Heal, (int)Mathf.Round(lifeSteal * finalDamages));
                    m_Controller.ClientAnalytics.SendSpellDataClientRPC(m_SpellData.Name, EHitType.LifeSteal, (int)Mathf.Round(lifeSteal * finalDamages));
                }
            }

            // if spell is AutoAttack & controller has a "AutoAttackRune" : add effects of the rune to the spell
            if (m_Controller.RuneData.GetType() == typeof(AutoAttackRune) && m_SpellData.Name == m_Controller.SpellHandler.AutoAttack.ToString())
            {
                ((AutoAttackRune)m_Controller.RuneData).ApplyOnHit(ref controller, m_Controller);
            }

            // apply state effects specifics to enemies
            ApplyEnemyStateEffects(controller);

            return true;
        }

        /// <summary>
        /// Check if ability hit an ally
        /// </summary>
        /// <param name="controller"> controller of hit target </param>
        /// <returns></returns>
        protected virtual bool CheckHitAlly(Controller controller)
        {
            if (controller.Team != m_Controller.Team)
                return false;

            if (m_SpellData.Heal <= 0 && m_SpellData.AllyStateEffects.Count == 0)
                return false;

            controller.Life.Heal(m_SpellData.Heal);
            
            if (m_Controller.ClientAnalytics != null)
                m_Controller.ClientAnalytics.SendSpellDataClientRPC(m_SpellData.Name, EHitType.Heal, m_SpellData.Heal);
            
            ApplyAllyStateEffects(controller);

            return true;
        }

        /// <summary>
        /// Set the value of the target, update direction and rotation
        /// </summary>
        /// <param name="target"></param>
        protected virtual void SetTarget(Vector3 target)
        {
            m_Target = target;
        }

        #endregion


        #region Spell Effects

        /// <summary>
        /// Spawn prefabs that procs on hitting a target
        /// </summary>
        /// <param name="targetController"></param>
        protected virtual void SpawnOnHitPrefab()
        {
            if (! IsServer)
                return;

            var position = transform.position;
            position.y = 0;
            m_SpellData.SpawnOnHitPrefab(OwnerClientId, position, position);
        }

        protected virtual void ApplyStateEffects(Controller targetController, List<SStateEffectData> stateEffects)
        {
            if (!IsServer)
                return;

            if (!targetController.Life.IsAlive)
                return;

            foreach (var effect in stateEffects)
            {
                targetController.StateHandler.AddStateEffect(effect, m_Controller, m_SpellData.Level);
            }
        }

        /// <summary>
        /// Apply on hit effects targetting enemies
        /// </summary>
        /// <param name="targetController"></param>
        protected virtual void ApplyEnemyStateEffects(Controller targetController)
        {
            ApplyStateEffects(targetController, m_SpellData.EnemyStateEffects);
        }

        /// <summary>
        /// Apply on hit effects targetting allies
        /// </summary>
        /// <param name="targetController"></param>
        protected virtual void ApplyAllyStateEffects(Controller targetController)
        {
            ApplyStateEffects(targetController, m_SpellData.AllyStateEffects);
        }

        #endregion


        #region Targetting

        /// <summary>
        /// Get Controller for the target
        /// </summary>
        /// <returns></returns>
        protected virtual Controller GetTargetController()
        {
            if (! SpellData.IsAutoTarget)
                return null;

            switch (SpellData.SpellTarget)
            {
                case ESpellTarget.Self:
                    return m_Controller;

                case ESpellTarget.FirstAlly:
                    return GameManager.Instance.GetFirstAlly(m_Controller.Team, OwnerClientId);

                case ESpellTarget.FirstEnemy:
                    return GameManager.Instance.GetFirstEnemy(m_Controller.Team);

                default:
                    ErrorHandler.Error("Unhandled case : " + SpellData.SpellTarget);
                    return null;
            }
        }

        protected virtual void RecalculateTarget(ref Transform baseTarget)
        {
            Controller targetController = GetTargetController();
            if ( targetController != null )
                baseTarget = targetController.transform;
        }

        #endregion


        #region Tools

        protected bool TryGetController(Collider2D collider, out Controller controller)
        {
            controller = null;
            if (collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                return false;

            // check that players has controller 
            controller = Finder.FindComponent<Controller>(collider.gameObject);
            if (controller == null)
            {
                ErrorHandler.Error("Controller not found for player " + collider.gameObject.name);
                return false;
            }

            return true;
        }

        #endregion


        #region Debug

        public virtual void DebugMessage()
        {
            Debug.Log("Spell " + m_SpellData.name);
            Debug.Log("     + ClientId " + OwnerClientId);
            Debug.Log("     + Target " + m_Target);
        }

        #endregion
    }
}