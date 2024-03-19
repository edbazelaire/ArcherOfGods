using Enums;
using System.ComponentModel;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "AutoAttackRune", menuName = "Game/Runes/AutoAttack")]
    public class AutoAttackRun : RuneData
    {
        public void OnHit(Controller controller)
        {
            controller.StateHandler.AddStateEffect(StateEffect);
        }

    }
}