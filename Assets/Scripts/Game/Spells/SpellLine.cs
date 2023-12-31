using Data;
using Enums;
using Game.Managers;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class SpellLine : Spell
    {
        #region Members

        #endregion


        #region Init & End

        public override void Initialize(Vector3 target, ESpells spellType)
        {
            base.Initialize(target, spellType);

            // set target to be align with orginal position
            target.y = transform.position.y;
            SetTarget(target);
        }

        #endregion
    }
}