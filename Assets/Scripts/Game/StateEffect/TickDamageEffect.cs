﻿using Data;
using Enums;
using System.Collections;
using Tools;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "TickDamageEffect", menuName = "Game/StateEffects/TickDamage")]
    public class TickDamageEffect : DamageEffect
    {

        #region Members

        [SerializeField] protected float    m_Tick;
        [SerializeField] protected int      m_TickDamages;
        [SerializeField] protected int      m_TickHeal;
        [SerializeField] protected int      m_TickShield;

        private float m_TickTimer;

        #endregion


        #region Inherited Manipulators

        public override bool Initialize(Controller controller, Controller caster, SStateEffectData? stateEffect)
        {
            if (!base.Initialize(controller, caster, stateEffect))
                return false;

            m_TickTimer = m_Tick;
            return true;
        }

        public override void Update()
        {
            m_TickTimer -= Time.deltaTime;

            if (m_TickTimer < 0 && m_Tick > 0)
            {
                ApplyTickEffects();
                m_TickTimer = m_Tick;
            }

            base.Update();
        }

        protected virtual void ApplyTickEffects()
        {
            int damages = GetInt(EStateEffectProperty.TickDamages);
            if (damages > 0)
            {
                ErrorHandler.Log($"{name} : {damages} DAMAGES", ELogTag.StateEffects);
                m_Controller.Life.Hit(damages, true);
            }

            int heal = GetInt(EStateEffectProperty.TickHeal);
            if (heal > 0)
            {
                m_Controller.Life.Heal(heal);
            }

            // add bonus tick shield
            m_RemainingShield += GetInt(EStateEffectProperty.TickShield);
        }

        #endregion
    }
}