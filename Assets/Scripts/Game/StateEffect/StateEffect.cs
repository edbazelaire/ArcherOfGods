using Data;
using Enums;
using System;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "StateEffect", menuName = "Game/StateEffects/Default")]
    [System.Serializable]
    public class StateEffect : ScriptableObject
    {
        #region Members

        // =========================================================================================
        // SERIALIZED DATA
        [SerializeField] protected      float                   m_Duration;
        [SerializeField] protected      int                     m_MaxStacks         = 1;
        [SerializeField] protected      float                   m_SpeedBonus        = 0f;
        [SerializeField] protected      int                     m_ResistanceFix     = 0;
        [SerializeField] protected      float                   m_ResistancePerc    = 1f;

        // =========================================================================================
        // PROTECTED MEMBERS   
        protected Controller            m_Controller;
        protected                       EStateEffect            m_Type;

        protected                       int                     m_Stacks;   
        protected                       int                     m_RemainingShield;
        protected                       float                   m_Timer;

        // =========================================================================================
        // DEPENDENT MEMBERS  
        public EStateEffect Type                => Enum.TryParse(name, out EStateEffect type) ? type : m_Type ;
        public float        Duration            => m_Duration;
        public int          RemainingShield     => m_RemainingShield;
        public float        SpeedBonus          => m_SpeedBonus;
        public int          Stacks              => m_Stacks;
        public bool         IsInfinite          => m_Duration <= 0;
        public int          MaxStacks           => m_MaxStacks;
        public int          ResistanceFix       => m_ResistanceFix;
        public float        ResistancePerc      => m_ResistancePerc;

        #endregion

        public virtual void Initialize(Controller controller, SStateEffectData stateEffect)
        {
            m_Controller = controller;
            name = stateEffect.StateEffect.ToString();

            // override duration if any provided
            if (stateEffect.OverrideDuration)
                m_Duration = stateEffect.Duration;

            // override SpeedBonus if any provided
            if (stateEffect.OverrideSpeedBonus)
                m_SpeedBonus = stateEffect.SpeedBonus;

            m_Stacks = stateEffect.Stacks;

            RefreshStats();
        }

        protected virtual void RefreshStats()
        {
            m_Timer             = m_Duration;
        }

        public virtual void End()
        {
            m_Controller.StateHandler.RemoveState(Type);
        }

        public virtual void UpdateTimer()
        {
            if (IsInfinite)
                return;

            m_Timer -= Time.deltaTime;

            if (m_Timer <= 0)
                End();
        }

        /// <summary>
        /// Hit the shield with some damages and return the remaining damages
        /// </summary>
        /// <param name="damages"></param>
        /// <returns></returns>
        public virtual int HitShield(int damages)
        {
            m_RemainingShield -= damages;
            if (m_RemainingShield >= 0)
                return 0;

            damages = -m_RemainingShield;
            m_RemainingShield = 0;
            return damages;
        }

        public virtual void Refresh(int stacks = 0)
        {
            if (m_MaxStacks > 0 && m_Stacks < m_MaxStacks) 
                m_Stacks = Math.Min(m_MaxStacks, m_Stacks + stacks);

            RefreshStats();
        }
    }
}