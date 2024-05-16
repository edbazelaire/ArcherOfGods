using Enums;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "Consume", menuName = "Game/StateEffects/Consume")]
    public class ConsumeEffect : DamageEffect
    {
        [Header("Consume State")]
        [Description("State to consume in order to apply effect")]
        [SerializeField] protected EStateEffect ConsumeState;

        [Description("State applied if the consume state is not found")]
        [SerializeField] protected EStateEffect DefaultState;

        /// <summary>
        /// Check that Frost state is applied, otherwise apply it instead of Frozen state
        /// </summary>
        /// <returns></returns>
        protected override bool CheckBeforeGraphicInit()
        {
            if (! base.CheckBeforeGraphicInit())
                return false;

            // can only apply to enemy with required state 
            if (!m_Controller.StateHandler.HasState(ConsumeState))
            {
                // add DefaultState state (if any)
                if (DefaultState != EStateEffect.None)
                    m_Controller.StateHandler.AddStateEffect(DefaultState);

                // return that this state can not be applied
                return false;
            }

            // consume "ConsumeState" state to apply current state
            m_Controller.StateHandler.RemoveState(ConsumeState);

            return true;
        }
    }
}