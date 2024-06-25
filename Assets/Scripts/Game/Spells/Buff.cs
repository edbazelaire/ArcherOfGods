using Data;
using Enums;
using Game.Loaders;
using System.Linq;
using Tools;
using UnityEngine;

namespace Game.Spells
{
    public class Buff : Spell
    {
        #region Members

        BuffData m_SpellData => m_BaseSpellData as BuffData;

        #endregion


        #region Init & End

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellName"></param>
        public override void Initialize(ulong clientId, Vector3 target, string spellName, int level)
        {
            base.Initialize(clientId, target, spellName, level);

            if (!IsServer)
                return;

            OnHitPlayer(GetTargetController());
            End();
        }

        #endregion


        #region Hitting

        protected override void OnHitPlayer(Controller controller) 
        {
            // add state effect specific to this spell (must have same name)
            controller.StateHandler.AddStateEffect(m_SpellData.GetStateEffect(), m_Controller);

            base.OnHitPlayer(controller);
        }

        #endregion
    }
}