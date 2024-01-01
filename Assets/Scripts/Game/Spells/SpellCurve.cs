using Enums;
using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Spells
{
    public class SpellCurve : Spell
    {
        #region Members

        float m_MaxHeight;
        float m_MaxDistance;

        #endregion


        #region Init & End

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void Initialize(Vector3 target, ESpells spellType)
        {
            base.Initialize(target, spellType);

            m_MaxHeight = GameManager.Instance.TargetHight.transform.position.y;
            m_MaxDistance = m_Target.x - m_OriginalPosition.x;
        }

        protected override void End()
        {
            Destroy(gameObject);
        }

        #endregion


        #region Override Manipulators

        /// <summary>
        /// 
        /// </summary>
        protected override void UpdateMovement()
        {
            // calculate next position
            var x       = Mathf.MoveTowards(transform.position.x, m_Target.x, m_Speed * Time.deltaTime);
            var baseY   = Mathf.Lerp(m_OriginalPosition.y, m_Target.y, (x - m_OriginalPosition.x) / m_MaxDistance);
            var height  = m_MaxHeight * Math.Abs(x - m_OriginalPosition.x) * Math.Abs(x - m_Target.x) / (0.25f * m_MaxDistance * m_MaxDistance);

            // update rotation to look at next position
            LookAt(new Vector3(x, baseY + height, transform.position.z));

            // update movement
            base.UpdateMovement();
        }

        #endregion
    }
}