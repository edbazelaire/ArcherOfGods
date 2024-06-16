using Enums;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "StateEffect", menuName = "Game/StateEffects/Aura/MissingLifeAura")]
    public class MissingLifeAura : StateEffect
    {
        #region Members

        [Header("Missing Life")]
        [SerializeField] protected float m_MissingLifeFactor;
        
        #endregion


        #region Data Accessors

        public override int GetInt(EStateEffectProperty property)
        {
            if (m_Controller == null)
                return base.GetInt(property);

            return (int)Mathf.Round(ApplyMissingLifeFactor(base.GetInt(property), m_Controller.Life.Hp.Value, m_Controller.Life.MaxHp.Value));
        }

        public override float GetFloat(EStateEffectProperty property)
        {
            if (m_Controller == null)
                return base.GetFloat(property);
          
            return ApplyMissingLifeFactor(base.GetFloat(property), m_Controller.Life.Hp.Value, m_Controller.Life.MaxHp.Value);
        }

        #endregion


        #region Private Method

        float ApplyMissingLifeFactor(float baseValue, int hp, int maxHp)
        {
            return m_MissingLifeFactor * (1 - (hp / maxHp)) * baseValue;
        }

        #endregion


        #region Infos

        /// <summary>
        /// Handle the info management of special cases 
        /// </summary>
        /// <param name="infosDict"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected override bool GetSpecialPropertiesInfos(ref Dictionary<string, object> infosDict, EStateEffectProperty property)
        {
            switch (property)
            {
                // skip this one
                case EStateEffectProperty.MissingLifeFactor:
                    return true;

                default:
                    return base.GetSpecialPropertiesInfos(ref infosDict, property);
            }
        }

        #endregion

    }
}