using Enums;
using NUnit.Framework.Internal;
using UnityEngine;

namespace Game.Spells
{
    public class Torment : Aoe
    {
        const int   DAMAGES_PER_STACKS = 5;
        const int   ENERGY_PER_STACKS = 2;

        #region Protected Manipulators
        
        protected override void ApplyEnemyOnHitEffects(Controller targetController)
        {
            Debug.Log("Torment.ApplyEnemyOnHitEffects()");

            // if has no cursed effect, apply it 
            if (! targetController.StateHandler.HasState(EStateEffect.Cursed))
            {
                base.ApplyEnemyOnHitEffects(targetController);
                return;
            }

            // if has cursed effects consumme them to apply damages and stung
            int nStacks = targetController.StateHandler.RemoveState(EStateEffect.Cursed);
            targetController.Life.Hit(nStacks * DAMAGES_PER_STACKS);
            m_Controller.EnergyHandler.AddEnergy(nStacks * ENERGY_PER_STACKS);

            // stun
            targetController.StateHandler.AddStateEffect(EStateEffect.Stun, duration: 2f);
        }

        #endregion
    }
}