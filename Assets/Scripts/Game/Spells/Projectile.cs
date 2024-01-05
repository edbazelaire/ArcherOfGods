﻿using Enums;
using System;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Game.Spells
{
    public class Projectile : Spell
    {
        #region Members

        float m_MaxHeight;
        float m_MaxDistance;

        Vector3 m_OriginalPosition;

        #endregion

        #region Init & End

        public override void Initialize(Vector3 target, ESpell spellType)
        {
            base.Initialize(target, spellType);

            m_OriginalPosition = transform.position;

            switch (m_SpellData.Trajectory)
            {
                case ESpellTrajectory.Curve:
                    m_MaxHeight = GameManager.Instance.TargetHight.transform.position.y;
                    m_MaxDistance = m_Target.x - m_OriginalPosition.x;
                    break;

                case ESpellTrajectory.Straight:
                    // set target to be align with orginal position
                    target.y = transform.position.y;
                    SetTarget(target);
                    break;

                case ESpellTrajectory.Hight:
                    break;

                default:
                    ErrorHandler.Error("Projectile::Initialize() - Unknown trajectory type " + m_SpellData.Trajectory);
                    break;
            }
        }

        #endregion


        #region Inherited Manipulators

        /// <summary>
        /// [SERVER] check for collision with wall or player
        /// </summary>
        /// <param name="collision"></param>
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            // only server can check for collision
            if (!IsServer)
                return;

            // if spell hits a wall, end it
            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
                End();

            // if spell hits a player, hit it and end the spell
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && !m_IsAoe)
                OnHitPlayer(Finder.FindComponent<Controller>(collision.gameObject));
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void UpdateMovement()
        {
            switch (m_SpellData.Trajectory)
            {
                case ESpellTrajectory.Curve:
                    UpdateCurveMovement();
                    break;
            }

            // all clients update the position of the spell (previsualisation)
            transform.Translate(m_SpellData.Speed * Time.deltaTime, 0, 0);

            // only server can check for distance
            if (!IsServer)
                return;

            // check if the spell has reached its max distance
            if (m_SpellData.Distance > 0 && Math.Abs(transform.position.x - m_OriginalPosition.x) > m_SpellData.Distance)
                End();
        }

        protected override void SetTarget(Vector3 target)
        {
            base.SetTarget(target);
            LookAt(m_Target);
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// Rotate the projectile to look at the target
        /// </summary>
        void UpdateCurveMovement()
        {
            // calculate next position
            var x = Mathf.MoveTowards(transform.position.x, m_Target.x, m_SpellData.Speed * Time.deltaTime);
            var baseY = Mathf.Lerp(m_OriginalPosition.y, m_Target.y, (x - m_OriginalPosition.x) / m_MaxDistance);
            var height = m_MaxHeight * Math.Abs(x - m_OriginalPosition.x) * Math.Abs(x - m_Target.x) / (0.25f * m_MaxDistance * m_MaxDistance);

            // update rotation to look at next position
            LookAt(new Vector3(x, baseY + height, transform.position.z));
        }

        /// <summary>
        /// Set rotation of the spell to look at the target
        /// </summary>
        /// <param name="target"></param>
        void LookAt(Vector3 target)
        {
            Vector3 diff = target - transform.position;
            diff.Normalize();
            float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z);
        }

        #endregion


        #region Dependent Properties

        bool m_IsAoe => m_SpellData.Radius > 0;

        #endregion
    }
}