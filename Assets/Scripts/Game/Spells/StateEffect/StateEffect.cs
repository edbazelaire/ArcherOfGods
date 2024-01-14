using Assets.Scripts.Data;
using Enums;
using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Game.Spells
{
    /// <summary>
    ///
    /// </summary>
    public class StateEffect 
    {
        protected Controller            m_Controller;
        protected SStateEffectData      m_StateEffectData;

        protected int                   m_RemainingShield;    
        protected float                 m_Timer;

        public SStateEffectData StateEffectData => m_StateEffectData;
        public int RemainingShield => m_RemainingShield;

        public StateEffect(Controller controller, SStateEffectData stateEffectData)
        {
            m_Controller        = controller;
            m_StateEffectData   = stateEffectData;

            Reset();
        }

        void InitStats()
        {
            m_Timer = m_StateEffectData.Duration;
            m_RemainingShield = m_StateEffectData.Shield;
        }

        public virtual void End()
        {
            return;
        }

        public virtual bool Update(float deltaTime)
        {
            if (m_StateEffectData.IsInfinite)
                return true;

            m_Timer -= deltaTime;

            if (m_Timer < 0)
            {
                End();
                return false;
            }

            // if has a shield but the shield is done : return that the effect is done
            if (m_RemainingShield <= 0 && m_StateEffectData.Shield > 0)
            {
                End();
                return false;
            }

            return true;
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

        public virtual void Reset()
        {
            InitStats();
        }
    }
}