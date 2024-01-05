using Assets.Scripts.Data;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class Aoe : NetworkBehaviour
    {
        #region Members

        NetworkVariable<float>  m_Radius    = new NetworkVariable<float>(0);
        NetworkVariable<int>    m_Damage    = new NetworkVariable<int>(0);
        NetworkVariable<float>  m_Duration  = new NetworkVariable<float>(0);

        private List<SStateEffectData> m_OnHitEffects;

        #endregion


        #region Init & End

        /// <summary>
        /// 
        /// </summary>
        public override void OnNetworkSpawn()
        {
            m_Radius.OnValueChanged += OnRadiusChanged;

            m_OnHitEffects = new List<SStateEffectData>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="damage"></param>
        /// <param name="duration"></param>
        public void Initialize(float radius, int damage, float duration, List<SStateEffectData> onHitEffects)
        {
            if (!IsServer)
                return;

            m_Radius.Value      = radius;
            m_Damage.Value      = damage;
            m_Duration.Value    = duration;

            m_OnHitEffects = onHitEffects;

            transform.localScale = new Vector3(m_Radius.Value, m_Radius.Value, 1f);
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
        protected void Update()
        {
            if (!IsServer)
                return;

            m_Duration.Value -= Time.deltaTime;
            if (m_Duration.Value <= 0f)
                Destroy(gameObject);
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
                var life = collision.gameObject.GetComponent<Life>();
                life.Hit(m_Damage.Value);

                ApplyOnHitEffects(Finder.FindComponent<Controller>(collision.gameObject));
            }
        }

        #endregion


        #region Private Manipulators
        
        void OnRadiusChanged(float oldRadius, float newRadius)
        {
            transform.localScale = new Vector3(newRadius, newRadius, 1f);
        }

        protected virtual void ApplyOnHitEffects(Controller targetController)
        {
            if (!IsServer)
                return;

            if (!targetController.Life.IsAlive)
                return;

            foreach (var effect in m_OnHitEffects)
            {
                targetController.StateHandler.AddStateEffect(effect);
            }
        }

        #endregion
    }
}