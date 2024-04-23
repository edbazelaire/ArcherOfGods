using Data;
using UnityEngine;

namespace Game.Spells
{
    public class InstantSpell : Spell
    {
        #region Members

        SpellData m_SpellData => m_BaseSpellData as SpellData;

        #endregion


        #region Inherited Manipulators

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

            OnHitPlayer(m_Controller);
            End();
        }

        #endregion
    }
}