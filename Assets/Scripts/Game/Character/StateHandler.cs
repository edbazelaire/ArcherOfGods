using Data;
using Enums;
using Game.Managers;
using Game.Spells;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class StateHandler : NetworkBehaviour
    {
        #region Members
        // ==============================================================================================
        // PRIVATE ACCESSORS
        // -- Network Variables
        NetworkList<FixedString64Bytes>     m_StateEffectList;
        NetworkVariable<float>              m_SpeedBonus = new(1f);
        NetworkVariable<int>                m_RemainingShield = new(0);

        // -- SERVER SIDE
        Controller              m_Controller;
        List<StateEffect>       m_StateEffects;

        // ==============================================================================================
        // PUBLIC ACCESSORS
        public NetworkList<FixedString64Bytes> StateEffectList => m_StateEffectList;
        public bool IsStunned => m_StateEffectList.Contains(EStateEffect.Stun.ToString());
        public NetworkVariable<float> SpeedBonus => m_SpeedBonus;
        public NetworkVariable<int> RemainingShield => m_RemainingShield;

        // ==============================================================================================
        // EVENTS
        public event Action<EListEvent, string, int, float> OnStateEvent;

        #endregion


        #region Init & End

        private void Awake()
        {
            // init network lists
            m_StateEffectList = new NetworkList<FixedString64Bytes>();

            // init components 
            m_Controller = GetComponent<Controller>();
            m_StateEffects = new List<StateEffect>();   
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_Controller.SpellHandler.AnimationTimer.OnValueChanged     += OnAnimationTimerChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            m_StateEffectList.Dispose();
        }


        public override void OnDestroy()
        {
            m_Controller.SpellHandler.AnimationTimer.OnValueChanged     -= OnAnimationTimerChanged;
        }

        #endregion


        #region Inherited Manipulators

        void Update()
        {
            // only server applies the effects
            if (! IsServer) 
                return;

            for (int i = m_StateEffects.Count - 1; i >= 0; i--)
            {
                m_StateEffects[i].Update();
            }
        }

        #endregion


        #region Client RPC

        [ClientRpc]
        void OnStateEventClientRPC(EListEvent listEvent, string stateEffect, int stacks, float duration)
        {
            OnStateEvent?.Invoke(listEvent, stateEffect, stacks, duration);
        }

        #endregion


        #region Public Accessors

        public bool HasState(string state)
        {
            return m_StateEffectList.Contains(state);
        }

        public bool HasState(EStateEffect state)
        {
            return HasState(state.ToString());
        }

        public void SetStateJump(bool on)
        {
            if (!IsServer)
                return;

            m_Controller.Collider.enabled = !on;

            if (on)
                AddStateEffect(new SStateEffectData(EStateEffect.Jump, duration: -1));
            else
                RemoveState(EStateEffect.Jump);
        }

        public int ApplyResistance(int damages)
        {
            // apply res fix first
            damages = Math.Max(0, damages - GetInt(EStateEffectProperty.ResistanceFix));

            // apply percentage res
            damages = (int)Mathf.Round(damages * GetFloat(EStateEffectProperty.ResistancePerc));
            
            return damages;
        }

        public int ApplyBonusDamages(int damages)
        {
            // apply percentage res
            damages = Math.Max(0, damages + GetInt(EStateEffectProperty.BonusDamages));

            // apply res fix first
            return (int)Mathf.Round(damages * GetFloat(EStateEffectProperty.BonusDamagesPerc));   
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// When an animation starts, remove states that are not allowed
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="newValue"></param>
        void OnAnimationTimerChanged(float previousValue, float newValue)
        {
            if (previousValue > 0 || newValue <= 0)
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

            m_SpeedBonus.Value  = GetFloat(EStateEffectProperty.SpeedBonus);

            int baseValue = 0;
            foreach (var effect in m_StateEffects)
            {
                baseValue += effect.RemainingShield;
            }

            m_RemainingShield.Value = baseValue;
        }

        /// <summary>
        /// Refresh the state effect
        /// </summary>
        /// <param name="stateEffectName"></param>
        void RefreshEffect(string stateEffectName, int stacks = 1)
        {
            foreach (var effect in m_StateEffects)
            {
                if (effect.StateEffectName != stateEffectName)
                    continue;

                effect.Refresh(stacks);
                OnStateEventClientRPC(EListEvent.Add, effect.StateEffectName, effect.Stacks, effect.GetFloat(EStateEffectProperty.Duration));
                RecalculateBonus();
                break;
            }
        }

        #endregion


        #region Public Manipulators

        /// <summary>
        /// Add a state effect to the character
        /// </summary>
        /// <param name="stateEffect"></param>
        public void AddStateEffect(SStateEffectData stateEffectData, int level = 1)
        {
            if (! IsServer)
                return;

            // create and add state effect  
            StateEffect stateEffect = SpellLoader.GetStateEffect(stateEffectData.StateEffect, level);
            AddStateEffect(stateEffect, stateEffectData);
        }

        public void AddStateEffect(StateEffect stateEffect, SStateEffectData? overridingData = null)
        {
            if (!IsServer)
                return;

            // if already in the list of state effects, refresh it
            if (HasState(stateEffect.StateEffectName))
            {
                RefreshEffect(stateEffect.StateEffectName, overridingData != null ? overridingData.Value.Stacks : 1);
                return;
            }

            if (! stateEffect.Initialize(m_Controller, overridingData))
                return;

            // add the state effect to the list of active effects
            m_StateEffects.Add(stateEffect);
            m_StateEffectList.Add(stateEffect.StateEffectName);

            // send event to clients (for UI update)
            OnStateEventClientRPC(EListEvent.Add, stateEffect.StateEffectName, stateEffect.Stacks, stateEffect.GetFloat(EStateEffectProperty.Duration));

            // recheck bonus potentially provided by this new stateEffect
            RecalculateBonus();
        }

        /// <summary>
        /// Add a state effect to the character
        /// </summary>
        /// <param name="type"></param>
        /// <param name="duration"></param>
        public void AddStateEffect(EStateEffect type, int? stacks = default, float? duration = default, float? speedBonus = default)
        {
            if (!IsServer)
                return;

            AddStateEffect(new SStateEffectData(
                type, 
                stacks:     stacks      ??      1,
                duration:   duration    ??     -1f,
                speedBonus: speedBonus  ??      0           
            ));
        }

        /// <summary>
        /// Remove a state effect from the character
        /// </summary>
        /// <param name="state"></param>
        public int RemoveState(string state)
        {
            if (!IsServer)
                return 0;

            // remove effect type from list of active effects
            int index = m_StateEffectList.IndexOf(state);
            if (index == -1)
            {
                ErrorHandler.FatalError($"Unable to find state {state} in list");
                return 0;
            }

            // keep track of the number of stacks this spell had
            int nStacks = m_StateEffects[index].Stacks;

            // send event to clients (for UI update)
            OnStateEventClientRPC(EListEvent.Remove, m_StateEffects[index].StateEffectName, m_StateEffects[index].Stacks, 0f);

            // remove effect from list on Server side
            Destroy(m_StateEffects[index]);
            m_StateEffectList.RemoveAt(index);
            m_StateEffects.RemoveAt(index);

            // recalculate bonuses givent by state effects
            RecalculateBonus();

            // return number of stacks this spell had (can be used when a spell consumes the state)
            return nStacks;
        }

        /// <summary>
        /// Remove a state effect from the character
        /// </summary>
        /// <param name="state"></param>
        public int RemoveState(EStateEffect state)
        {
            return RemoveState(state.ToString());
        }

        /// <summary>
        /// Hit the shield with some damages and return the remaining damages
        /// </summary>
        /// <param name="damages"></param>
        /// <returns></returns>
        public int HitShield(int damages)
        {
            if (m_RemainingShield.Value == 0)
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


        #region Public Data Accessors

        public float GetFloat(EStateEffectProperty property)
        {
            // only server can calculate speed factor
            if (!IsServer)
                return 1f;

            float baseValue = 1f;

            foreach (var effect in m_StateEffects)
            {
                baseValue += effect.GetFloat(property);
            }

            return baseValue;
        }

        public int GetInt(EStateEffectProperty property)
        {
            // only server can calculate speed factor
            if (!IsServer)
                return 0;

            int baseValue = 0;

            foreach (var effect in m_StateEffects)
            {
                baseValue += effect.GetInt(property);
            }

            return baseValue;
        }

        #endregion

    }
}