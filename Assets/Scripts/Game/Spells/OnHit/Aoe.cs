using Assets.Scripts.Data;
using Enums;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class Aoe : Spell
    {
        #region Members

        NetworkVariable<float>  m_Radius    = new NetworkVariable<float>(0);

        float m_DurationTimer;

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
        public override void Initialize(Vector3 target, string spellName)
        {
            base.Initialize(target, spellName);

            if (!IsServer)
                return;

            m_Radius.Value = m_SpellData.Size;
            m_DurationTimer = m_SpellData.Duration;
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


        #region Inherited Manipulators

        /// <summary>
        /// 
        /// </summary>
        protected override void Update()
        {
            if (!IsServer)
                return;

            m_DurationTimer -= Time.deltaTime;
            if (m_DurationTimer <= 0f)
                End();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collision"></param>
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer)
                return;

            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                OnHitPlayer(Finder.FindComponent<Controller>(collision.gameObject));
            }
        }

        #endregion


        #region Private Manipulators
        
        void OnRadiusChanged(float oldRadius, float newRadius)
        {
            transform.localScale = new Vector3(newRadius, newRadius, 1f);
        }

       

        #endregion
    }
}