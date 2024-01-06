using Assets.Scripts.Data;
using Enums;
using System.Collections;
using UnityEngine;

namespace Game.Spells
{
    public class BurnEffect : TickDamageEffect
    {
        #region Constructor

        public BurnEffect(Controller controller, SStateEffectData onHitData) : base(controller, onHitData)
        {
            return;
        }

        #endregion


        #region Inherited Manipulators

        public override bool Update(float deltaTime)
        {
            return base.Update(deltaTime);
        }

        #endregion
    }
}