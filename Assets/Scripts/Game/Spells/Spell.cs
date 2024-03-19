using Data;
using Enums;
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
                
        /// <summary> counter of remaining number of target that this spell can hit </summary>
        protected List<ulong>       m_HittedPlayerId;
        /// <summary> on cast prefabs (spawn on cast) </summary>
        protected List<GameObject>  m_OnCastPrefabs;

        // ========================================================================================================
        // Events
        public Action OnSpellEndedEvent;


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
        public virtual void Initialize(Vector3 target, string spellName, int level)
        {
            m_Controller        = GameManager.Instance.GetPlayer(OwnerClientId);
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
                Debug.Log("InitGraphics() : " + m_SpellData.Graphics + " with size " + m_SpellData.Size);
                Instantiate(m_SpellData.Graphics, m_GraphicsContainer.transform);
            }

            transform.localScale = new Vector3(m_SpellData.Size, m_SpellData.Size, 1f);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void End()
        {
            // visual ending effect
            SpawnOnHitPrefab();

            // destroy the spell game object
            DestroySpell();
        }

        public virtual void DestroySpell()
        {
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
        public void InitializeClientRpc(Vector3 target, string spellName, int level)
        {
            if (IsHost)
                return;
            Initialize(target, spellName, level);
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
            if (controller.Team == m_Controller.Team)
                return false;

            if (m_SpellData.Damage <= 0 && m_SpellData.EnemyStateEffects.Count == 0 && m_SpellData.OnHit.Count == 0)
                return false;

            int damages = m_Controller.StateHandler.ApplyBonusDamages(m_SpellData.Damage);
            if (controller.CounterHandler.HasCounter)
            {
                controller.CounterHandler.ProcCounter(this);
                return false;
            }
            
            // get final damages after shields and resistances
            int finalDamages = controller.Life.Hit(damages);

            // apply lifesteal if any (remove 1 because floats values are always based on 1 as default value)
            float lifeSteal = Mathf.Max(0f, m_Controller.StateHandler.GetFloat(EStateEffectProperty.LifeSteal) - 1);

            if (lifeSteal > 0 && finalDamages > 0)
            {
                m_Controller.Life.Heal((int)Mathf.Round(lifeSteal * finalDamages));
            }
            
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
                targetController.StateHandler.AddStateEffect(effect, m_SpellData.Level);
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
                    return GameManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId);

                case ESpellTarget.FirstAlly:
                    return GameManager.Instance.GetFirstAlly(GameManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).Team, NetworkManager.Singleton.LocalClientId);

                case ESpellTarget.FirstEnemy:
                    return GameManager.Instance.GetFirstEnemy(GameManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId).Team);

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