using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "AutoAttackRune", menuName = "Game/Runes/AutoAttack")]
    public class AutoAttackRune : RuneData
    {
        public void ApplyOnHit(ref Controller controller, Controller caster)
        {
            controller.StateHandler.AddStateEffect(StateEffect, caster, m_Level); ;
        }
    }
}