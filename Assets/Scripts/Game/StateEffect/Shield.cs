using UnityEditor;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "ShieldEffect", menuName = "Game/StateEffects/Shields")]
    public class ShieldEffect : StateEffect
    {
        public override void UpdateTimer()
        {
            base.UpdateTimer();

            if (m_RemainingShield == 0)
                End();
        }   
    }
}