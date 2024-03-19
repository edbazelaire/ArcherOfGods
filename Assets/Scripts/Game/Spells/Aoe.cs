﻿using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class Aoe : Spell
    {
        #region Members

        protected virtual float DELAY_END_DURATION => 1f;

        AoeData m_SpellData => m_BaseSpellData as AoeData;

        protected NetworkVariable<float>  m_Radius    = new NetworkVariable<float>(0);

        protected float m_DurationTimer;

        #endregion


        #region Init & End

        /// <summary>
        /// 
        /// </summary>
        public override void OnNetworkSpawn()
        {
            m_Radius.OnValueChanged += OnRadiusChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="damage"></param>
        /// <param name="duration"></param>
        public override void Initialize(Vector3 target, string spellName, int level)
        {
            // handle size and position (before graphic initialization)
            transform.position = new Vector3(target.x, 0f, 1f);

            base.Initialize(target, spellName, level);

            transform.localScale = new Vector3(m_SpellData.Size, m_SpellData.Size, 1f);

            if (!IsServer)
                return;

            // setup radius and timer
            m_Radius.Value      = m_SpellData.Size;
            m_DurationTimer     = m_SpellData.Duration;

            CreateCollisionCircle();
        }

        protected override void End()
        {
            // visual ending effect
            SpawnOnHitPrefab();

            // destroy the spell game object (with delay if any)
            StartCoroutine(DelayEndGraphics());
        }

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        public override void OnDestroy()
        {
            m_Radius.OnValueChanged -= OnRadiusChanged;
            base.OnDestroy();
        }

        #endregion


        #region Update

        /// <summary>
        /// 
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (!IsServer)
                return;

            m_DurationTimer -= Time.deltaTime;
            if (m_DurationTimer <= 0f)
            {
                End();
                return;
            }
        }

        #endregion


        #region Collision Manipulators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collision"></param>
        protected virtual void OnCollision(Collider2D collision)
        {
            if (!IsServer)
                return;

            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // check that players has controller 
                var controller = Finder.FindComponent<Controller>(collision.gameObject);
                if (controller == null)
                {
                    ErrorHandler.Error("Controller not found for player " + collision.gameObject.name);
                    return;
                }

                OnCollisionController(controller);
            }
        }

        protected virtual void OnCollisionController(Controller controller)
        {
            Debug.Log("OnHit() : " + m_SpellData.SpellName);
            Debug.Log("     + Owner : " + OwnerClientId);
            Debug.Log("     + LocalId : " + NetworkManager.Singleton.LocalClientId);

            // hit the player
            OnHitPlayer(controller);
        }

        protected void CreateCollisionCircle()
        {
            // Check for collisions within a circle with variableRadius radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, m_Radius.Value);

            // Iterate through all colliders found
            foreach (Collider2D collider in colliders)
            {
                OnCollision(collider);
            }
        }

        #endregion


        #region Graphics Manipulation

        protected virtual IEnumerator DelayEndGraphics()
        {
            var timer = DELAY_END_DURATION;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            DestroySpell();
        }

        #endregion


        #region Listeners

        protected virtual void OnRadiusChanged(float oldRadius, float newRadius)
        {
            transform.localScale = new Vector3(newRadius, newRadius, 1f);
        }

        #endregion
    }
}