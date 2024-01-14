using Data;
using Enums;
using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Game.Spells
{
    public class Jump : Projectile
    {
        #region Members

        bool m_StartEnd = false;
        float m_DurationTimer;
        float m_OffsetY;

        #endregion

        // Use this for initialization
        public override void Initialize(Vector3 target, string spellName)
        {
            base.Initialize(target, spellName);

            m_OffsetY = 0.1f + ((CapsuleCollider2D)m_Controller.Collider).size.y / 2;

            transform.position = m_Controller.transform.position;
            m_OriginalPosition = transform.position;
            m_MaxDistance = Math.Abs(m_Target.x - m_OriginalPosition.x);

            m_DurationTimer = m_SpellData.Duration;

            // make player untargatable, unmovable and unrotatable
            if (IsServer)
                m_Controller.StateHandler.SetStateJump(true);
        }

        // Update is called once per frame
        protected override void Update()
        {
            if (m_StartEnd)
            {
                CheckEnd();
                return;
            }

            base.Update();

            // only server can check for distance and update the Controller position
            if (!IsServer)
                return;

            Vector3 pos = transform.position;
            pos.y += m_OffsetY;
            m_Controller.transform.position = pos;

            // check if the spell has reached its max distance
            if (Math.Abs(transform.position.x - m_OriginalPosition.x) >= m_MaxDistance)
            {
                // visual ending effect
                SpawnOnHitPrefab();

                // re activate collider
                m_Controller.Collider.enabled = true;

                // reset jump state
                m_Controller.StateHandler.SetStateJump(false);

                // start end counter
                m_StartEnd = true;
            }
        }

        protected override void OnTriggerEnter2D(Collider2D collistion)
        {
            return;
        }


        #region Protected Members


        #endregion


        #region Private Members

        void CheckEnd()
        {
            if (m_DurationTimer <= 0)
            {
                // reset player position
                m_Controller.transform.position = m_OriginalPosition;
                DestroySpell();
            }
            
            m_DurationTimer -= Time.deltaTime;
        }

        #endregion
    }
}