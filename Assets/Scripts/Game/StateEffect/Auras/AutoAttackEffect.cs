﻿using Data;
using Enums;
using Game.Loaders;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "AutoAttackEffect", menuName = "Game/StateEffects/AutoAttackEffect")]
    public class AutoAttackEffect : SpellEffect
    {
        [Header("Auto Attack")]
        [SerializeField] protected SpellData                m_ReplacementData;

        // ==============================================================================
        // DATA
        protected ESpell m_ReplacedSpell = ESpell.Count;

        #region Init & End

        public override bool Initialize(Controller controller, Controller caster, SStateEffectData? stateEffectData = null)
        {
            if (!base.Initialize(controller, caster, stateEffectData))
                return false;

            if (m_ReplacementData != null)
            {
                if (m_ReplacementData.SpellType == ESpellType.MultiProjectiles)
                {
                    var autoAttackData = SpellLoader.GetSpellData(m_Controller.SpellHandler.AutoAttack);
                    if (autoAttackData.SpellType != ESpellType.Projectile)
                    {
                        ErrorHandler.Error("Unhandled case : trying to set multiprojectile AutoAttack BUFF on a non projectile auto attack");
                        return false;
                    }

                    (m_ReplacementData as MultiProjectilesData).ProjectileData = (SpellLoader.GetSpellData(m_Controller.SpellHandler.AutoAttack) as ProjectileData);
                    (m_ReplacementData as MultiProjectilesData).OverrideProjectile((SpellLoader.GetSpellData(m_Controller.SpellHandler.AutoAttack) as ProjectileData));
                }

                m_ReplacedSpell = m_Controller.SpellHandler.AutoAttack;
                m_Controller.SpellHandler.ReplaceSpell(m_Controller.SpellHandler.AutoAttack, m_ReplacementData);
            }

            return true;
        }

        public override void End()
        {
            if (m_ReplacementData != null)
                m_Controller.SpellHandler.RemoveOverridingSpell(m_Controller.SpellHandler.AutoAttack);

            base.End(); 
        }

        #endregion

    }
}