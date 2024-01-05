using Assets.Scripts.Data;
using System.Collections;
using UnityEngine;

namespace Game.Spells
{
    public class DamageEffect : StateEffect
    {
        protected int m_Damages;
        protected int m_Shield;
        protected int m_Heal;

        public DamageEffect(Controller controller, SStateEffectData onHitData) : base(controller, onHitData)
        {
            m_Damages   = onHitData.Damages;
            m_Shield    = onHitData.Shield;
            m_Heal      = onHitData.Heal;
        }

    }
}