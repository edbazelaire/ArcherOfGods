using Enums;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class SpellCurve : Spell
    {
        #region Members

        //NetworkVariable<Vector3>    m_TargetLookAt = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        Vector3 m_TargetLookAt;

        #endregion


        #region Init & End

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SetCurrentTarget(GameManager.Instance.TargetHight.transform.position);
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
            if (m_Target != m_TargetLookAt && Math.Abs(transform.position.y - m_TargetLookAt.y) < 0.1)
                SetCurrentTarget(m_Target);

            LookAt(m_TargetLookAt);
            m_Direction = (m_TargetLookAt - transform.position).normalized;

            base.UpdateMovement();
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        void SetCurrentTarget(Vector3 target)
        {
            m_TargetLookAt = target;
        }

        #endregion
    }
}