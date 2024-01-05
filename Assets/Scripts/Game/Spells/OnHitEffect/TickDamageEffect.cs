using Assets.Scripts.Data;
using Enums;
using System.Collections;
using UnityEngine;

namespace Game.Spells
{
    public class TickDamageEffect : DamageEffect
    {

        #region Members

        protected float m_Tick;

        private float m_TickTimer;

        #endregion


        #region Constructor

        public TickDamageEffect(Controller controller, SStateEffectData onHitData) : base(controller, onHitData)
        {
            m_Tick = onHitData.Tick;
            m_TickTimer = m_Tick;
        }

        #endregion


        #region Inherited Manipulators

        public override bool Update(float deltaTime)
        {
            m_TickTimer -= deltaTime;

            if (m_TickTimer < 0 && m_Tick > 0)
            {
                ApplyTickEffects();
                m_TickTimer = m_Tick;
            }

            return base.Update(deltaTime);
        }

        protected virtual void ApplyTickEffects()
        {
            m_Controller.Life.Hit(m_Damages);
        }

        #endregion
    }
}