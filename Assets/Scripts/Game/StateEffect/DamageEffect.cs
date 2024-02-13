using Data;
using Enums;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "Game/StateEffects/DamageEffect")]
    public class DamageEffect : StateEffect
    {
        [SerializeField] protected int m_Damages;
        [SerializeField] protected int m_Heal;

        /// <summary>
        /// Apply damages / Heal on end
        /// </summary>
        public override void End()
        {
            m_Controller.Life.Hit(m_Damages);
            m_Controller.Life.Heal(m_Heal);

            base.End();
        }

    }
}