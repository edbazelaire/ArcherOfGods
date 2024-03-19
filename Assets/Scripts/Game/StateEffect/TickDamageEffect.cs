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

        [SerializeField] protected float    m_Tick;
        [SerializeField] protected int      m_TickDamages;
        [SerializeField] protected int      m_TickHeal;
        [SerializeField] protected int      m_TickShield;

        private float m_TickTimer;

        #endregion


        #region Inherited Manipulators

        public override bool Initialize(Controller controller, SStateEffectData? stateEffect)
        {
            if (!base.Initialize(controller, stateEffect))
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
            m_Controller.Life.Hit(m_TickDamages * m_Stacks, true);
            m_Controller.Life.Heal(m_TickHeal * m_Stacks);

            // add bonus tick shield
            m_RemainingShield += m_TickShield;
        }

        #endregion
    }
}