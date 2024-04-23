using NUnit.Framework.Internal;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "AutoAttackRune", menuName = "Game/Runes/AutoAttack")]
    public class AutoAttackRune : RuneData
    {
        public void ApplyOnHit(ref Controller controller)
        {
            controller.StateHandler.AddStateEffect(StateEffect, m_Level); ;
        }
    }
}