using Enums;
using System;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "Frost", menuName = "Game/StateEffects/Frost")]
    public class Frost : StateEffect
    {
        protected override bool CheckBeforeGraphicInit()
        {
            // cant apply frost states when enemy is frozen
            if (m_Controller.StateHandler.HasState(EStateEffect.Frozen))
            {
                return false;
            }

            return base.CheckBeforeGraphicInit();
        }
    }
}