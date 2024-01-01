using Data;
using Enums;
using Game.Managers;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class Spell : NetworkBehaviour
    {
        #region Members

        // ownership
        protected Controller        m_Controller;
        protected ulong             m_ClientId;

        // spell data
        protected ESpells           m_SpellType;
        protected float             m_Speed;
        protected int               m_Damage;
        protected float             m_Distance;

        // targetting
        protected Vector3           m_Target;
        protected Vector3           m_OriginalPosition;

        #endregion


        #region Init & End

        /// <summary>
        /// Called when the spell is spawned on the network
        /// </summary>
        public override void OnNetworkSpawn()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_SpellType);
            
            m_Controller        = GameManager.Instance.GetPlayer(OwnerClientId);
            m_ClientId          = OwnerClientId;
            m_Speed             = spellData.Speed;
            m_Damage            = spellData.Damage;
            m_Distance          = spellData.Distance;
            m_OriginalPosition  = transform.position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellType"></param>
        public virtual void Initialize(Vector3 target, ESpells spellType)
        {
            m_SpellType = spellType;
            SetTarget(target);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void End()
        {
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
        public void InitializeClientRPC(Vector3 target, ESpells spellType)
        {
            Initialize(target, spellType);
        }

        #endregion


        #region Inherited Manipulators  

        /// <summary>
        /// 
        /// </summary>
        protected void Update()
        {
            UpdateMovement();
        }

        /// <summary>
        /// [SERVER] check for collision with wall or player
        /// </summary>
        /// <param name="collision"></param>
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            // only server can check for collision
            if (! IsServer)
                return;

            // if spell hits a wall, end it
            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                End();
            }

            // if spell hits a player, hit it and end the spell
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Controller controller = collision.gameObject.GetComponent<Controller>();
                if (controller.Team == m_Controller.Team)
                    return;

                controller.Life.Hit(m_Damage);
                End();
            }
        }

        #endregion


        #region Protected Manipulators

        /// <summary>
        /// Update the position of the spell and [SERVER] check if the spell has reached its max distance
        /// </summary>
        protected virtual void UpdateMovement()
        {
            // all clients update the position of the spell (previsualisation)
            transform.Translate(m_Speed * Time.deltaTime, 0, 0);

            // only server can check for distance
            if (!IsServer)
                return;

            // check if the spell has reached its max distance
            if (m_Distance > 0 && Math.Abs(transform.position.x - m_OriginalPosition.x) > m_Distance)
                End();
        }

        /// <summary>
        /// Set the value of the target, update direction and rotation
        /// </summary>
        /// <param name="target"></param>
        protected virtual void SetTarget(Vector3 target)
        {
            m_Target = target;
            LookAt(m_Target);
        }

        /// <summary>
        /// Set rotation of the spell to look at the target
        /// </summary>
        /// <param name="target"></param>
        protected virtual void LookAt(Vector3 target)
        {
            Vector3 diff = target - transform.position;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z);
        }

        #endregion


        #region Debug

        public void DebugMessage()
        {
            Debug.Log("Spell " + m_SpellType);
            Debug.Log("     + ClientId " + m_ClientId);
            Debug.Log("     + Target " + m_Target);
            Debug.Log("     + Speed " + m_Speed);    
        }

        #endregion
    }
}