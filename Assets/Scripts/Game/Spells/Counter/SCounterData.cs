using Enums;
using MyBox;
using System.ComponentModel;
using UnityEngine;

namespace Game.Spells
{
    [System.Serializable]
    public struct SCounterData
    {
        [Description("Spell Caster when the counter procs")]
        public          ECounterType Type;
        
        [Description("Duration of the counter")]
        public          float Duration;

        [Description("Speed bonus provided by the counter (NOT IMPLEMENTED YET)")]
        public          float SpeedBonus;

        [Description("Duration of the counter")]
        public          int MaxHit;
        
        [Description("Spell Caster when the counter procs")]
        [ConditionalField("Type", false, ECounterType.Proc)]
        public          ESpell OnCounterProc;

        [Header("Graphics")]
        public          Color       ColorSwap;   
        public          GameObject  CounterGraphics;

        
        public SCounterData(ECounterType type = ECounterType.None, float duration = 0, float speedBonus = 0, int maxHit = 1, ESpell onCounterProc = ESpell.Count, Color colorSwap = default, GameObject counterGraphics = null)
        {
            Type = type;
            Duration = duration;
            SpeedBonus = speedBonus;
            MaxHit = maxHit;
            OnCounterProc = onCounterProc;
            ColorSwap = colorSwap;
            CounterGraphics = counterGraphics;
        }
    }
}