using Enums;
using MyBox;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    [System.Serializable]
    public struct SStateEffectData
    {
        [Header("General")]
        public EStateEffect         Type;
        public float                Duration;
        public float                SpeedBonus;
        public bool                 IsInfinite;    

        [ConditionalField("Type", true, EStateEffect.Stun)]
        public int      Damages;
        [ConditionalField("Type", true, EStateEffect.Stun)]
        public int      Shield;
        [ConditionalField("Type", true, EStateEffect.Stun)]
        public int      Heal;

        [ConditionalField("Type", true, EStateEffect.Stun)]
        public float    Tick;

        public SStateEffectData(EStateEffect type, float duration = 0, float speedBonus = 0, bool isInfinite = false, int damages = 0, int shield = 0, int heal = 0, float tick = 0)
        {
            Type = type;
            Duration = duration;
            SpeedBonus = speedBonus;
            IsInfinite = isInfinite;
            Damages = damages;
            Shield = shield;
            Heal = heal;
            Tick = tick;
        }
    }
}