using Data;
using Enums;
using System.Collections;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "TickDamageEffect", menuName = "Game/StateEffects/TickDamage")]
    public class TickDamageEffect : DamageEffect
    {

        #region Members

        [SerializeField] protected float m_Tick;
        [SerializeField] protected int m_TickDamage;
        [SerializeField] protected int m_TickHeal;
        [SerializeField] protected int m_TickShield;

        private float m_TickTimer;

        #endregion


        #region Inherited Manipulators

        public override void Initialize(Controller controller, SStateEffectData stateEffect)
        {
            base.Initialize(controller, stateEffect);

            m_TickTimer = m_Tick;
        }

        public override void UpdateTimer()
        {
            m_TickTimer -= Time.deltaTime;

            if (m_TickTimer < 0 && m_Tick > 0)
            {
                ApplyTickEffects();
                m_TickTimer = m_Tick;
            }

            base.UpdateTimer();
        }

        protected virtual void ApplyTickEffects()
        {
            m_Controller.Life.Hit(m_TickDamage * m_Stacks, true);
            m_Controller.Life.Heal(m_TickHeal * m_Stacks);

            // add bonus tick shield
            m_RemainingShield += m_TickShield;
        }

        #endregion
    }
}