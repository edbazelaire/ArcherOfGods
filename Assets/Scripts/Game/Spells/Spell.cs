using Data;
using Enums;
using Game.Character;
using Game.Managers;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class Spell : NetworkBehaviour
    {
        #region Members

        const string c_GraphicsContainer = "GraphicsContainer";

        protected Controller        m_Controller;
        protected SpellData         m_SpellData;
        protected Vector3           m_Target;
        protected GameObject        m_GraphicsContainer;

        /// <summary> counter of remaining number of target that this spell can hit </summary>
        protected int               m_SpellHitCount;
        /// <summary> on cast prefabs (spawn on cast) </summary>
        protected List<GameObject>  m_OnCastPrefabs;

        #endregion


        #region Init & End

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spell"></param>
        public virtual void Initialize(Vector3 target, ESpell spell)
        {
            m_Controller    =  GameManager.Instance.GetPlayer(OwnerClientId);
            m_SpellData     =  SpellLoader.GetSpellData(spell);
            SetTarget(target);

            m_SpellHitCount = m_SpellData.MaxHit;
            InitGraphics();
        }

        /// <summary>
        /// Instantiate the graphics of the spell
        /// </summary>
        protected virtual void InitGraphics()
        {
            m_GraphicsContainer = Finder.Find(gameObject, c_GraphicsContainer);

            if (m_SpellData.Graphics != null)
                GameObject.Instantiate(m_SpellData.Graphics, m_GraphicsContainer.transform);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void End()
        {
            // visual ending effect
            SpawnOnHitPrefab();

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
        public void InitializeClientRpc(Vector3 target, ESpell spellType)
        {
            Initialize(target, spellType);
            
            m_OnCastPrefabs = m_SpellData.SpawnOnCastPrefabs(m_Controller.gameObject.transform, m_Target);
        }

        [ClientRpc]
        public void EndClientRpc()
        {
            foreach (var prefab in m_OnCastPrefabs)
                Destroy(prefab);
        }

        #endregion


        #region Inherited Manipulators  

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Update()
        {
            UpdateMovement();
        }

        #endregion


        #region Protected Manipulators

        /// <summary>
        /// Update the position of the spell and [SERVER] check if the spell has reached its max distance
        /// </summary>
        protected virtual void UpdateMovement()
        {
            return;
        }

        protected virtual void OnHitPlayer(Controller controller)
        {
            // not alive : skip
            if (!controller.Life.IsAlive)
                return;

            // apply effects on ally or enemy : if none, skip
            if (! CheckHitEnemy(controller) && ! CheckHitAlly(controller))
                return;

            // energy gain
            m_Controller.EnergyHandler.AddEnergy(m_SpellData.EnergyGain);

            // update hit count
            if (--m_SpellHitCount <= 0 && m_SpellData.MaxHit > 0)
                End();
        }

        /// <summary>
        /// Check if ability hit an enemy
        /// </summary>
        /// <param name="controller"> controller of hit target </param>
        /// <returns></returns>
        protected virtual bool CheckHitEnemy(Controller controller)
        {
            if (controller.Team == m_Controller.Team)
                return false;

            if (m_SpellData.Damage <= 0 && m_SpellData.EnemyStateEffects.Count == 0)
                return false;

            if (controller.CounterHandler.HasCounter)
            {
                controller.CounterHandler.ProcCounter(m_Controller);
                End();
            }
            else
                controller.Life.Hit(m_SpellData.Damage);

            ApplyEnemyOnHitEffects(controller);

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
            ApplyAllyOnHitEffects(controller);

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

        protected virtual void SpawnOnHitPrefab()
        {
            if (! IsServer)
                return;

            var position = transform.position;
            position.y = 0;
            m_SpellData.SpawnOnHitPrefab(OwnerClientId, position);
        }

        protected virtual void ApplyEnemyOnHitEffects(Controller targetController)
        {
            if (! IsServer)
                return;

            if (!targetController.Life.IsAlive)
                return;

            foreach (var effect in m_SpellData.EnemyStateEffects)
            {
                targetController.StateHandler.AddStateEffect(effect);
            }
        }

        protected virtual void ApplyAllyOnHitEffects(Controller targetController)
        {
            if (! IsServer)
                return;

            if (!targetController.Life.IsAlive)
                return;

            foreach (var effect in m_SpellData.AllyStateEffects)
            {
                targetController.StateHandler.AddStateEffect(effect);
            }
        }


        #endregion


        #region Debug

        public virtual void DebugMessage()
        {
            Debug.Log("Spell " + m_SpellData.Spell);
            Debug.Log("     + ClientId " + OwnerClientId);
            Debug.Log("     + Target " + m_Target);
        }

        #endregion
    }
}