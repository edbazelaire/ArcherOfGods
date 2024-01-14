using Enums;
using System.Collections;
using System.Linq;
using Tools;
using UnityEngine;

namespace Game.Spells
{
    public class InstantSpell : Spell
    {
        ESpellType[] COUNTER_PROCABLE_SPELLTYPE = { ESpellType.Projectile };

        public override void Initialize(Vector3 target, string spellName)
        {
            base.Initialize(target, spellName);

            if (!IsServer)
                return;

            switch (m_SpellData.SpellType)
            {
                case ESpellType.Counter:
                    m_Controller.CounterHandler.SetCounter(m_SpellData.CounterData);
                    break;

                case ESpellType.InstantSpell:
                    OnHitPlayer(m_Controller);
                    End();
                    break;

                default:
                    ErrorHandler.Error("InstantSpell::Initialize() - Unknown spell type " + m_SpellData.SpellType);
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (m_SpellData.SpellType == ESpellType.Counter)
                transform.position = m_Controller.transform.position;

            if (! IsServer)
                return;

            if (m_SpellData.SpellType == ESpellType.Counter)
                CheckCounterUpdate();
        }

        protected virtual void CheckCounterUpdate()
        {
            if (! m_Controller.CounterHandler.HasCounter)
                End();
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer)
                return;

            if (m_SpellData.SpellType == ESpellType.Counter)
            {
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
        }
    }
}