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

        [ConditionalField("Type", true, EStateEffect.Stun)]
        public int      Damages;
        [ConditionalField("Type", true, EStateEffect.Stun)]
        public int      Shield;
        [ConditionalField("Type", true, EStateEffect.Stun)]
        public int      Heal;

        [ConditionalField("Type", true, EStateEffect.Stun)]
        public float    Tick;
    }
}