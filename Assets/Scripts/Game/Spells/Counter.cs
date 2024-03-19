using Data;
using Enums;
using Game.Managers;
using System.Linq;
using Tools;
using UnityEngine;

namespace Game.Spells
{
    public class Counter : Spell
    {
        #region Members

        ESpellType[] COUNTER_PROCABLE_SPELLTYPE = { ESpellType.Projectile };

        CounterData m_SpellData => m_BaseSpellData as CounterData;

        #endregion


        #region Inherited Manipulators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spellName"></param>
        public override void Initialize(Vector3 target, string spellName, int level)
        {
            base.Initialize(target, spellName, level);

            if (!IsServer)
                return;

            m_Controller.CounterHandler.SetCounter(m_SpellData);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Update()
        {
            base.Update();

            transform.position = m_Controller.transform.position;

            if (! IsServer)
                return;

            CheckCounterUpdate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collision"></param>
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer)
                return;
  
            // check if is spell
            if (collision.gameObject.layer != LayerMask.NameToLayer("Spell"))
                return;

            // check if spell can proc counter
            Spell spell = Finder.FindComponent<Spell>(collision.gameObject);

            // check that this is not a spell from the same team
            if (spell.Controller.Team == m_Controller.Team)
                return;
                
            // check that spell can proc counter
            if (!COUNTER_PROCABLE_SPELLTYPE.Contains(spell.SpellData.SpellType))
                return;

            m_Controller.CounterHandler.ProcCounter(spell);
        }

        #endregion


        #region Protected Manipulators

        protected virtual void CheckCounterUpdate()
        {
            if (!m_Controller.CounterHandler.HasCounter)
                End();
        }

        #endregion
    }
}