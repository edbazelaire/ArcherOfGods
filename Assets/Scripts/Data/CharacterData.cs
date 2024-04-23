using Enums;
using Game.Loaders;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct SCharacterStatScaling
    {
        public EStateEffectProperty StateEffectProperty;
        public float BaseValue;
        public float ScalingFactor;

        public SCharacterStatScaling(EStateEffectProperty stateEffectProperty, float baseValue, float scalingFactor = 1.1f)
        {
            StateEffectProperty = stateEffectProperty;
            BaseValue           = baseValue;
            ScalingFactor       = scalingFactor;
        }
    }

    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : CollectionData
    {
        [Header("Spells")]
        public ESpell           AutoAttack;
        public ESpell           Ultimate;

        [Header("Stats")]
        public float            Size        = 1f;
        public float            Speed       = 3f;
        public int              MaxHealth   = 100;
        public int              MaxEnergy   = 100;

        [Header("Bonus Stats")]
        public List<SCharacterStatScaling> CharacterStatScaling;

        protected override Type m_EnumType => typeof(ECharacter);
        public ECharacter Character => (ECharacter)Id;

        public GameObject InstantiateCharacterPreview(GameObject parent)
        {
            var go = GameObject.Instantiate(AssetLoader.LoadCharacterPreview(Character), parent.transform);
            return go;
        }

        public new CharacterData Clone(int level = 0)
        {
            return (CharacterData)base.Clone(level);
        }

        protected override void SetLevel(int level)
        {
            // TODO : personal factors
            float scaleFactor = 1.1f;

            var currentFactor = Math.Pow(scaleFactor, m_Level - 1);
            var newFactor = Math.Pow(scaleFactor, level - 1);

            MaxHealth = (int)Math.Round(MaxHealth * newFactor / currentFactor);

            base.SetLevel(level);
        }

        public float GetValue(EStateEffectProperty property) 
        {
            var characterStatScalingData = GetCharacterScalingData(property);
            if (! characterStatScalingData.HasValue)
                return 0.0f;

            return Mathf.Pow(1f + characterStatScalingData.Value.ScalingFactor, m_Level - 1) * characterStatScalingData.Value.BaseValue;
        }

        public int GetInt(EStateEffectProperty property) 
        {
            return (int)Math.Round(GetValue(property));
        }

        SCharacterStatScaling? GetCharacterScalingData(EStateEffectProperty property)
        {
            foreach(SCharacterStatScaling data in CharacterStatScaling)
            {
                if (data.StateEffectProperty == property)
                    return data;
            }

            return null;
        }


        #region Infos

        public override Dictionary<string, object> GetInfos()
        {
            var infosDict = base.GetInfos();

            infosDict.Add("Health", MaxHealth);
            infosDict.Add("MovementSpeed", Speed);

            foreach (SCharacterStatScaling data in CharacterStatScaling)
            {
                infosDict.Add(data.StateEffectProperty.ToString(), GetValue(data.StateEffectProperty));
            }

            return infosDict;
        }

        #endregion
    }
}