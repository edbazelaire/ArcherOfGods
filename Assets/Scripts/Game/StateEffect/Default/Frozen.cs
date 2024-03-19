using Data;
using Enums;
using System;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "Frozen", menuName = "Game/StateEffects/Frozen")]
    public class Frozen : StateEffect
    {
        public override void Update()
        {
            base.Update();

            if (m_RemainingShield <= 0)
                m_Controller.StateHandler.RemoveState(StateEffectName);
        }

        /// <summary>
        /// Check that Frost state is applied, otherwise apply it instead of Frozen state
        /// </summary>
        /// <returns></returns>
        protected override bool CheckBeforeGraphicInit()
        {
            // can only apply to enemy with state "Frost"
            if (! m_Controller.StateHandler.HasState(EStateEffect.Frost))
            {
                // add frost state
                m_Controller.StateHandler.AddStateEffect(EStateEffect.Frost);
                return false;
            }

            // consume "Frost" state to apply "Frozen" state
            m_Controller.StateHandler.RemoveState(EStateEffect.Frost);

            return true;
        }
    }
}