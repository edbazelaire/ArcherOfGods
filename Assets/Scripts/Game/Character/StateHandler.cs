using Data;
using Enums;
using Game.Managers;
using Game.Spells;
using Game.UI;
using MyBox;
using System;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Character
{
    public class StateHandler : NetworkBehaviour
    {
        #region Members
        // ==============================================================================================
        // PRIVATE ACCESSORS
        // -- Network Variables
        NetworkList<int>        m_StateEffectList;
        NetworkVariable<float>  m_SpeedBonus = new(1f);
        NetworkVariable<int>    m_Shield = new(0);

        // -- SERVER SIDE
        Controller              m_Controller;
        List<StateEffect>       m_StateEffects;
        int                     m_ResistanceFix     = 0;
        float                   m_ResistancePerc    = 1f;

        // ==============================================================================================
        // PUBLIC ACCESSORS
        public NetworkList<int> StateEffectList => m_StateEffectList;
        public bool IsStunned => m_StateEffectList.Contains((int)EStateEffect.Stun);
        public NetworkVariable<float> SpeedBonus => m_SpeedBonus;
        public int Shield => m_Shield.Value;

        // ==============================================================================================
        // EVENTS
        public event Action<EListEvent, EStateEffect, int, float> OnStateEvent;

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
                m_StateEffects[i].UpdateTimer();
            }
        }

        #endregion


        #region Client RPC

        [ClientRpc]
        void OnStateEventClientRPC(EListEvent listEvent, EStateEffect stateEffect, int stacks, float duration)
        {
            OnStateEvent?.Invoke(listEvent, stateEffect, stacks, duration);
        }

        #endregion


        #region Public Accessors

        public bool HasState(EStateEffect state)
        {
            return m_StateEffectList.Contains((int)state);
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
            damages = Math.Max(0, damages - m_ResistanceFix);

            // apply percentage res
            damages = (int)Mathf.Round(damages * m_ResistancePerc);
            
            return damages;
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

            int shield          = 0;
            float speedBonus    = 1f;
            int resFix          = 0;
            float resPerc       = 1f;

            foreach (var effect in m_StateEffects)
            {
                // some stack effects are set at 0 but the minimum value of the factor is 1
                int stacksFactor = Math.Max(1, effect.Stacks);

                shield      += effect.RemainingShield;
                speedBonus  *= Mathf.Pow(effect.SpeedBonus, stacksFactor);
                resFix      += stacksFactor * effect.ResistanceFix;
                resPerc     *= Mathf.Pow(effect.ResistancePerc, stacksFactor);
            }

            m_SpeedBonus.Value  = speedBonus;
            m_Shield.Value      = shield;
            m_ResistanceFix     = resFix;
            m_ResistancePerc    = resPerc;
        }

        /// <summary>
        /// Refresh the state effect
        /// </summary>
        /// <param name="stateEffect"></param>
        void RefreshEffect(EStateEffect stateEffect, int stacks = 0)
        {
            foreach (var effect in m_StateEffects)
            {
                if (effect.Type != stateEffect)
                    continue;

                effect.Refresh(stacks);
                OnStateEventClientRPC(EListEvent.Add, effect.Type, effect.Stacks, effect.Duration);
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
        public void AddStateEffect(SStateEffectData stateEffectData)
        {
            if (! IsServer)
                return;

            // if already in the list of state effects, refresh it
            if (HasState(stateEffectData.StateEffect))
            {
                RefreshEffect(stateEffectData.StateEffect, stateEffectData.Stacks);
                return;
            }

            // create and add state effect  
            StateEffect stateEffect = SpellLoader.GetStateEffect(stateEffectData.StateEffect);
            stateEffect.Initialize(m_Controller, stateEffectData);
            m_StateEffects.Add(stateEffect);

            // add the state effect to the list of active effects
            m_StateEffectList.Add((int)stateEffectData.StateEffect);

            // send event to clients (for UI update)
            OnStateEventClientRPC(EListEvent.Add, stateEffect.Type, stateEffect.Stacks, stateEffect.Duration);

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
        public int RemoveState(EStateEffect state)
        {
            if (!IsServer)
                return 0;

            // remove effect type from list of active effects
            int index = m_StateEffectList.IndexOf((int)state);
            if (index == -1)
            {
                ErrorHandler.FatalError($"Unable to find state {state} in list");
                return 0;
            }

            // keep track of the number of stacks this spell had
            int nStacks = m_StateEffects[index].Stacks;

            // send event to clients (for UI update)
            OnStateEventClientRPC(EListEvent.Remove, m_StateEffects[index].Type, m_StateEffects[index].Stacks, m_StateEffects[index].Duration);

            // remove effect from list on Server side
            m_StateEffectList.RemoveAt(index);
            m_StateEffects.RemoveAt(index);

            // recalculate bonuses givent by state effects
            RecalculateBonus();

            // return number of stacks this spell had (can be used when a spell consumes the state)
            return nStacks;
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