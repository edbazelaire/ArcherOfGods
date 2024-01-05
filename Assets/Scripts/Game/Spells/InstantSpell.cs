using Enums;
using System.Collections;

namespace Game.Spells
{
    public class InstantSpell : Spell
    {
        protected override void Update()
        {
            base.Update();

            if (! IsServer)
                return;

            if (m_SpellData.SpellType == ESpellType.Counter)
            {
                m_Controller.CounterHandler.SetCounter(m_SpellData.OnCounterProc, m_SpellData.CounterDuration);
                End();
            } else
            {
                OnHitPlayer(m_Controller);
            }
            
        }
    }
}