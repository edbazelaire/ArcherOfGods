using Assets.Scripts.Data;
using Enums;
using Game.Spells;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class StateHandler : NetworkBehaviour
    {
        #region Members

        NetworkList<int>        m_StateEffectList;
        NetworkVariable<float>  m_SpeedBonus = new(0f);
        NetworkVariable<int>    m_Shield = new(0);

        Controller              m_Controller;
        List<StateEffect>       m_StateEffects;

        public NetworkList<int> StateEffectList => m_StateEffectList;
        public bool IsStunned => m_StateEffectList.Contains((int)EStateEffect.Stun);

        public float SpeedBonus => m_SpeedBonus.Value;
        public int Shield => m_Shield.Value;

        #endregion


        #region Init & End

        private void Awake()
        {
            // init network lists
            m_StateEffectList = new NetworkList<int>();

            // init components 
            m_Controller = GetComponent<Controller>();
            m_StateEffects = new List<StateEffect>();   
        }

        public override void OnNetworkSpawn()
        {
            m_StateEffectList.OnListChanged += OnHitEffectsListChanged;
            m_Controller.SpellHandler.AnimationTimer.OnValueChanged += OnAnimationTimerChanged;
        }

        public override void OnDestroy()
        {
            m_StateEffectList.OnListChanged -= OnHitEffectsListChanged;
            m_Controller.SpellHandler.AnimationTimer.OnValueChanged -= OnAnimationTimerChanged;
        }

        #endregion


        #region Inherited Manipulators

        void Update()
        {
            // only server applies the effects
            if (! IsServer) 
                return;

            bool removedDone = false;
            for (int i = m_StateEffects.Count - 1; i >= 0; i--)
            {
                bool removeState = false;

                if (! m_StateEffects[i].Update(Time.deltaTime))
                    removeState = true;

                if (!removeState)
                    continue;

                RemoveState(m_StateEffects[i].StateEffectData.Type, true);
                removedDone = true;
            }

            if (removedDone)
                RecalculateBonus();
        }

        #endregion

        
        #region Public Accessors

        public bool HasState(EStateEffect state)
        {
            return m_StateEffectList.Contains((int)state);
        }

        #endregion


        #region Private Manipulators

        void RemoveState(EStateEffect state, bool skipRecalculation = false)
        {
            // remove effect type from list of active effects
            int index = m_StateEffectList.IndexOf((int)state);
            if (index == -1)
            {
                ErrorHandler.FatalError($"Unable to find state {state} in list");
                return;
            }

            m_StateEffectList.RemoveAt(index);
            m_StateEffects.RemoveAt(index);

            if (! skipRecalculation)
                RecalculateBonus();
        }

        /// <summary>
        /// Client side impact of list changed
        /// </summary>
        /// <param name="changeEvent"></param>
        void OnHitEffectsListChanged(NetworkListEvent<int> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<int>.EventType.Add)
                RecalculateBonus();
        }

        /// <summary>
        /// When an animation starts, remove states that are not allowed
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="newValue"></param>
        void OnAnimationTimerChanged(float previousValue, float newValue)
        {
            if (previousValue > 0)
                return;

            if (HasState(EStateEffect.Invisible))
                RemoveState(EStateEffect.Invisible);
        }


        /// <summary>
        /// Calculate the total speed bonus provided by all OnHitEffects
        /// </summary>
        void RecalculateBonus()
        {
            // only server can calculate speed factor
            if (!IsServer)
                return;

            int shield = 0;
            float speedFactor = 0f;
            foreach (var effect in m_StateEffects)
            {
                speedFactor += effect.StateEffectData.SpeedBonus;
                shield      += effect.StateEffectData.Shield;
            }

            m_SpeedBonus.Value = speedFactor;
            m_Shield.Value = shield;
        }

        /// <summary>
        /// Refresh the state effect
        /// </summary>
        /// <param name="stateEffect"></param>
        void RefreshEffect(EStateEffect stateEffect)
        {
            foreach (var effect in m_StateEffects)
            {
                if (effect.StateEffectData.Type == stateEffect)
                    effect.Reset();
            }
        }

        #endregion


        #region Public Manipulators

        /// <summary>
        /// Add a state effect to the character
        /// </summary>
        /// <param name="stateEffectData"></param>
        public void AddStateEffect(SStateEffectData stateEffectData)
        {
            if (! IsServer)
                return;

            // if already in the list of state effects, refresh it
            if (HasState(stateEffectData.Type))
            {
                RefreshEffect(stateEffectData.Type);
                return;
            }

            // create the state effect from the data
            switch (stateEffectData.Type)
            {
                case EStateEffect.Poison:
                case EStateEffect.Burn:
                    m_StateEffects.Add(new TickDamageEffect(m_Controller, stateEffectData));
                    break;
                
                default:
                    m_StateEffects.Add(new StateEffect(m_Controller, stateEffectData));
                    break;
            }

            // add the state effect to the list of active effects
            m_StateEffectList.Add((int)stateEffectData.Type);
        }

        /// <summary>
        /// Hit the shield with some damages and return the remaining damages
        /// </summary>
        /// <param name="damages"></param>
        /// <returns></returns>
        public int HitShield(int damages)
        {
            if (Shield == 0)
                return damages;

            foreach (var effect in m_StateEffects)
            {
                damages = effect.HitShield(damages);
                if (damages == 0)
                    break;
            }

            RecalculateBonus();

            return damages;
        }

        #endregion


        #region Network Variables Accessors

        public List<EStateEffect> OnHitEffectList
        {
            get
            {
                var list = new List<EStateEffect>();
                foreach (var effect in m_StateEffectList)
                {
                    list.Add((EStateEffect)effect);
                }
                return list;
            }
        }

        #endregion
    }
}