﻿using Assets.Scripts.Managers.Sound;
using Data;
using Enums;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tools;
using UnityEngine;

namespace Game.Spells
{
    [Serializable]
    public struct SStateEffectScaling
    {
        public EStateEffectProperty     StateEffectProperty;
        public float                    ScalingFactor;

        public SStateEffectScaling(EStateEffectProperty stateEffectProperty, float scalingFactor = 1.1f)
        {
            StateEffectProperty     = stateEffectProperty;
            ScalingFactor           = scalingFactor;
        }
    }


    [CreateAssetMenu(fileName = "StateEffect", menuName = "Game/StateEffects/Default")]
    [System.Serializable]
    public class StateEffect : ScriptableObject
    {
        #region Members

        // =========================================================================================
        // SERIALIZED DATA
        [SerializeField] protected      string                      m_Description = "";
        [SerializeField] protected      List<EStateEffectProperty>  m_DescriptionVariables = new List<EStateEffectProperty>();

        [Header("Graphics")]
        [SerializeField] protected      GameObject                  m_VisualEffect;
        [SerializeField] protected      Color                       m_ColorSwitch = Color.white;
        [SerializeField] protected      EAnimation                  m_Animation;
        [SerializeField] protected      AudioClip                   m_OnApplySoundFX;
        [SerializeField] protected      AudioClip                   m_PermanantSoundFX;

        [Header("Consume State")]
        [SerializeField] protected EStateEffect                     m_ConsumeState;
        [ConditionalField("ConsumeState", true, EStateEffect.None)]
        [SerializeField] protected EStateEffect                     m_DefaultState;

        [Header("General Stats")]
        [SerializeField] protected      float                       m_Duration;
        [SerializeField] protected      int                         m_MaxStacks         = 1;

        [Header("General Boosts")]
        [SerializeField] protected      float                       m_SpeedBonus        = 0f;
        [SerializeField] protected      float                       m_CastSpeed         = 0f;
        [SerializeField] protected      float                       m_AttackSpeed       = 0f;

        [Header("Resistance & Shields")]
        [SerializeField] protected      int                         m_Shield            = 0;
        [SerializeField] protected      int                         m_ResistanceFix     = 0;
        [SerializeField] protected      float                       m_ResistancePerc    = 0f;

        [Header("Damages")]
        [SerializeField] protected      int                         m_BonusDamages      = 0;
        [SerializeField] protected      float                       m_BonusDamagesPerc  = 0f;
        [SerializeField] protected      float                       m_LifeSteal         = 0f;

        [Header("Level Scaling")]
        /// <summary> Scaling factor for each properties depending on number of Stacks for each levels </summary>
        [SerializeField] protected      List<SStateEffectScaling>   m_StateEffectScalingLevel   = new();

        /// <summary> Scaling factor for each properties depending on number of Stacks for each stats </summary>
        [SerializeField] protected      List<SStateEffectScaling>   m_StateEffectScalingStacks  = new();

        // =========================================================================================
        // PROTECTED MEMBERS   
        /// <summary> Controller on which the state effect is applied </summary>
        protected Controller            m_Controller;
        /// <summary> Controller that applied the state effect </summary>
        protected Controller            m_Caster;
        /// <summary> Level of the state effect </summary>
        protected int                   m_Level;
        /// <summary> Type of state effect </summary>
        protected EStateEffect          m_Type;
        protected AudioSource           m_AudioSource;

        protected int                   m_Stacks;   
        protected int                   m_RemainingShield;
        protected float                 m_Timer;

        // =========================================================================================
        // DEPENDENT MEMBERS  
        public GameObject               VisualEffect        => m_VisualEffect;
        public Color                    ColorSwitch         => m_ColorSwitch;
        public EAnimation               Animation           => m_Animation;
        public EStateEffect             Type                => Enum.TryParse(name, out EStateEffect type) ? type : m_Type ;
        public virtual int              Stacks              => m_Stacks;
        public virtual bool             IsInfinite          => m_Duration <= 0;
        public int                      RemainingShield     => m_RemainingShield;
        public virtual int              MaxStacks           => m_MaxStacks;
        public EStateEffect             ConsumeState        => m_ConsumeState;
        public EStateEffect             DefaultState        => m_DefaultState;

        public string StateEffectName
        {
            get
            {
                string myName = name;
                if (myName.EndsWith("(Clone)"))
                    myName = myName[..^"(Clone)".Length];

                return myName;
            }
        }

        #endregion


        #region Init & End

        public virtual bool Initialize(Controller controller, Controller caster, SStateEffectData? stateEffectData = null)
        {
            m_Controller = controller;
            m_Caster = caster;

            if (stateEffectData.HasValue)
                ApplyStateEffectData(stateEffectData.Value);

            if (!CheckBeforeGraphicInit())
            {
                return false;
            }

            m_RemainingShield = GetInt(EStateEffectProperty.Shield);

            RefreshStats();

            return true;
        }

        public virtual void End()
        {
            if (m_AudioSource != null)
            {
                Destroy(m_AudioSource);
            }

            m_Controller.StateHandler.RemoveState(StateEffectName);
        }


        #endregion


        #region At Init 

        /// <summary>
        /// Method that allows children to make a verification before instantiating this
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckBeforeGraphicInit()
        {
            if (m_ConsumeState == EStateEffect.None)
                return true;

            // can only apply to enemy with required state 
            if (!m_Controller.StateHandler.HasState(m_ConsumeState))
            {
                // add DefaultState state (if any)
                if (m_DefaultState != EStateEffect.None)
                    m_Controller.StateHandler.AddStateEffect(m_DefaultState, m_Caster);

                // return that this state can not be applied
                return false;
            }

            // consume "ConsumeState" state to apply current state
            m_Stacks = Math.Min(m_Controller.StateHandler.RemoveState(m_ConsumeState), m_MaxStacks);

            return true;
        }

        public void ApplyStateEffectData(SStateEffectData stateEffectData)
        {
            name = stateEffectData.StateEffect.ToString();

            // override duration if any provided
            if (stateEffectData.OverrideDuration)
                m_Duration = stateEffectData.Duration;

            // override SpeedBonus if any provided
            if (stateEffectData.OverrideSpeedBonus)
                m_SpeedBonus = stateEffectData.SpeedBonus;

            m_Stacks = stateEffectData.Stacks > 0 ? stateEffectData.Stacks : 1;
        }

        public void PlaySoundEffect()
        {
            if (m_OnApplySoundFX != null)
                SoundFXManager.PlayOnce(m_OnApplySoundFX);

            if (m_PermanantSoundFX != null)
                m_AudioSource = SoundFXManager.PlaySoundFXClip(m_PermanantSoundFX);
        }

        #endregion


        #region Update & Refresh

        public virtual void Update()
        {
            if (IsInfinite)
                return;

            m_Timer -= Time.deltaTime;

            if (m_Timer <= 0)
                End();
        }

        public virtual void Refresh(int stacks = 1)
        {
            if (m_MaxStacks <= 1)
                m_Stacks = 1;

            else if (m_Stacks < m_MaxStacks)
            {
                if (stacks == 0)
                    stacks = 1;

                m_Stacks = Math.Min(m_MaxStacks, m_Stacks + stacks);
            }

            RefreshStats();
        }

        protected virtual void RefreshStats()
        {
            m_Timer = m_Duration;
        }

        #endregion


        #region Shield

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

        #endregion


        #region Level Scaling Methods

        public StateEffect Clone(int level)
        {
            StateEffect clone = Instantiate(this);
            clone.name = this.name;
            clone.SetLevel(level);

            return clone;
        }

        protected void SetLevel(int level)
        {
            ApplyNewLevelFactorAll(level);
            m_Level = level;
        }

        protected virtual void ApplyNewLevelFactorAll(int level)
        {
            foreach (SStateEffectScaling stateEffectScaling in m_StateEffectScalingLevel)
            {
                ApplyNewLevelScalingEffectFactor(stateEffectScaling, level - 1, m_Level - 1);
            }
        }

        protected virtual void ApplyNewLevelFactor(EStateEffectProperty property, int level)
        {
            SStateEffectScaling stateEffectScaling = m_StateEffectScalingLevel.FirstOrDefault(effect => effect.StateEffectProperty == property);

            // check that a scaling value was provided
            if (stateEffectScaling.StateEffectProperty != property)
                return;

            if (stateEffectScaling.ScalingFactor <= 0f)
            {
                ErrorHandler.Warning("StateEffectProperty (" + stateEffectScaling.StateEffectProperty + ") was set with a ScalingFactor (" + stateEffectScaling.ScalingFactor + " ) < 0 for StateEffect " + name);
                return;
            }

            ApplyNewLevelScalingEffectFactor(stateEffectScaling, level - 1, m_Level - 1);
        }

        protected virtual void ApplyNewLevelScalingEffectFactor(SStateEffectScaling stateEffectScaling, int newLevel, int oldLevel)
        {
            // factor of current level
            float currentFactor = (float)Math.Pow(1 + stateEffectScaling.ScalingFactor, oldLevel);
            // factor of the level we are setting
            float newFactor = (float)Math.Pow(1 + stateEffectScaling.ScalingFactor, newLevel);

            // current factor is 0 -> set value to 0 to avoid division by 0
            if (currentFactor == 0)
            {
                ErrorHandler.Warning(string.Format("ScalingFactor ({0}) of property ({1}) of StateEffect {2} resulted in a division by 0", stateEffectScaling.ScalingFactor, stateEffectScaling.StateEffectProperty, name));
                SetProperty(stateEffectScaling.StateEffectProperty, 0);
                return;
            }

            // get info of the property
            if (!TryGetPropertyInfo(stateEffectScaling.StateEffectProperty, out FieldInfo propertyInfo))
                return;

            Type propertyType = propertyInfo.FieldType;
            if (propertyType == typeof(float))
            {
                var value = GetProperty<float>(stateEffectScaling.StateEffectProperty);
                SetProperty(stateEffectScaling.StateEffectProperty, value * newFactor / currentFactor);
                return;
            }

            if (propertyType == typeof(int))
            {
                var value = GetProperty<int>(stateEffectScaling.StateEffectProperty);
                SetProperty(stateEffectScaling.StateEffectProperty, (int)Math.Round(value * newFactor / currentFactor));
                return;
            }
        }

        #endregion


        #region Reflection Methods

        /// <summary>
        /// Get Reflection PropertyInfo of desire StateEffect property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected bool TryGetPropertyInfo(EStateEffectProperty property, out FieldInfo propertyInfo, bool throwError = true)
        {
            // Get the type of MyClass
            Type myStateEffectType = this.GetType();

            // Get the PropertyInfo object for the provided property
            propertyInfo = myStateEffectType.GetField("m_" + property.ToString(), BindingFlags.NonPublic | BindingFlags.Instance);

            // check if the property exists
            if (propertyInfo == null)
            {
                if (throwError)
                    ErrorHandler.Error("Unknown property " + property + " for StateEffect " + name);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the value of a property by Reflection
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected virtual void SetProperty(EStateEffectProperty property, object value)
        {
            if (!TryGetPropertyInfo(property, out FieldInfo propertyInfo))
                return;

            propertyInfo.SetValue(this, value);
        }

        /// <summary>
        /// Get the value of a property by Reflection
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual object GetProperty(EStateEffectProperty property)
        {
            if (!TryGetPropertyInfo(property, out FieldInfo propertyInfo))
                return null;

            return propertyInfo.GetValue(this);
        }

        protected virtual T GetProperty<T>(EStateEffectProperty property)
        {
            object value = GetProperty(property);
            if (value == null)
                return default;

            try
            {
                return (T)value;
            } catch (Exception ex)
            {
                ErrorHandler.Error(ex.Message);
                ErrorHandler.Error("Unable to parse value " + value + " of property " + property + " of state effect " + name + " into " + typeof(T));
                return default;
            }
            
        }

        #endregion


        #region Data Accessors

        public virtual int GetInt(EStateEffectProperty property) 
        {
            SStateEffectScaling stateEffectScalingStacks = m_StateEffectScalingStacks.FirstOrDefault(effect => effect.StateEffectProperty == property);
            SStateEffectScaling stateEffectScalingLevel = m_StateEffectScalingLevel.FirstOrDefault(effect => effect.StateEffectProperty == property);

            int baseValue = GetProperty<int>(property);                                             // Base Value of the property

            if (baseValue == 0)
                return 0;

            float levelFactor = stateEffectScalingLevel.StateEffectProperty == property ? Mathf.Pow(1f + stateEffectScalingLevel.ScalingFactor, m_Level) : 1f;
            int valueLevelScaled = (int)Mathf.Round(baseValue * levelFactor);
            int boostedValue = m_Controller.StateHandler.ApplyBonusInt(valueLevelScaled, property);        // Bonus values applied to the property
            float stacksFactor = stateEffectScalingStacks.StateEffectProperty == property ? Stacks * stateEffectScalingStacks.ScalingFactor : 1f;          // apply Stack bonus 

            ErrorHandler.Log("GetInt() : " + property, ELogTag.StateEffects);
            ErrorHandler.Log("      + Final Value : " + (int)Mathf.Round(boostedValue * stacksFactor), ELogTag.StateEffects);
            ErrorHandler.Log("      + baseValue : " + baseValue, ELogTag.StateEffects);
            ErrorHandler.Log("      + levelFactor : " + levelFactor, ELogTag.StateEffects);
            ErrorHandler.Log("      + valueLevelScaled : " + valueLevelScaled, ELogTag.StateEffects);
            ErrorHandler.Log("      + boostedValue : " + boostedValue, ELogTag.StateEffects);
            ErrorHandler.Log("      + stacksFactor : " + stacksFactor, ELogTag.StateEffects);

            // return boosted valye
            return (int)Mathf.Round(boostedValue * stacksFactor);    
        }

        public virtual float GetFloat(EStateEffectProperty property) 
        {
            SStateEffectScaling stateEffectScaling = m_StateEffectScalingStacks.FirstOrDefault(effect => effect.StateEffectProperty == property);

            // check that a scaling value was provided
            if (stateEffectScaling.StateEffectProperty != property || stateEffectScaling.ScalingFactor == 0)
                return GetProperty<float>(property);

            return Mathf.Round(100 * GetProperty<float>(property) * Stacks * (1f + stateEffectScaling.ScalingFactor)) / 100;
        }

        public virtual float GetBoostedValue(EStateEffectProperty property)
        {
            return m_Controller.StateHandler.ApplyBonus(GetProperty<float>(property), property);
        }

        public virtual int GetBoostedValueInt(EStateEffectProperty property)
        {
            return m_Controller.StateHandler.ApplyBonusInt(GetProperty<int>(property), property);
        }

        #endregion


        #region Infos

        public virtual Dictionary<string, object> GetInfos()
        {
            var infosDict = new Dictionary<string, object>();
            
            foreach (EStateEffectProperty property in Enum.GetValues(typeof(EStateEffectProperty)))
            {
                // check if property exists for this StateEffect and get reflection object
                if (! TryGetPropertyInfo(property, out FieldInfo propertyInfo, false))
                    continue;

                // handles special case properties
                if (GetSpecialPropertiesInfos(ref infosDict, property))
                    continue;

                if (propertyInfo.FieldType == typeof(float))
                {
                    float value = GetProperty<float>(property);
                    if (value != 0)
                        infosDict.Add(property.ToString(), value);
                }

                else if (propertyInfo.FieldType == typeof(int))
                {
                    int value = GetProperty<int>(property);

                    if (value == 0)
                        continue;
                   
                    infosDict.Add(property.ToString(), value);
                }
            }

            return infosDict;
        }

        /// <summary>
        /// Handle the info management of special cases 
        /// </summary>
        /// <param name="infosDict"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual bool GetSpecialPropertiesInfos(ref Dictionary<string, object> infosDict, EStateEffectProperty property)
        {
            switch (property)
            {
                case EStateEffectProperty.None:
                    return true;

                case EStateEffectProperty.MaxStacks:
                    var value = GetProperty<int>(property);
                    if (value > 1)
                        infosDict.Add(property.ToString(), value);
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Get Description info of the StateEffect
        /// </summary>
        /// <returns></returns>
        public virtual string GetDescription()
        {
            List<string> values = new List<string>();
            foreach(EStateEffectProperty property in m_DescriptionVariables)
            {
                var value = GetProperty(property);
                if (value == null)
                {
                    values.Add("UNDEFINED");
                    continue;
                }

                if (value is float floatValue)
                {
                    values.Add(floatValue.ToString("F2"));
                }
                else if (value is double doubleValue)
                {
                    values.Add(doubleValue.ToString("F2"));
                }
                else
                {
                    values.Add(value.ToString());
                }
            }

            return string.Format(m_Description, values.ToArray());
        }

        #endregion
    }
}