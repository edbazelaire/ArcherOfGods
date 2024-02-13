using Enums;
using System;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "Frost", menuName = "Game/StateEffects/Frost")]
    public class Frost : StateEffect
    {
        public float SpeedFactor => ( m_SpeedBonus < 0 ? -1 : 1 ) * Mathf.Pow(Mathf.Abs(m_SpeedBonus), m_Stacks);

        public override void Refresh(int stacks = 0)
        {
            base.Refresh(stacks);

            if (m_Stacks != m_MaxStacks)
                return;

            m_Controller.StateHandler.RemoveState(Type);
            m_Controller.StateHandler.AddStateEffect(EStateEffect.Stun, 3);
        }
    }
}