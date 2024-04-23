using Enums;
using Game.Spells;
using MyBox;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Counter", menuName = "Game/Spells/Counter")]
    public class CounterData : SpellData
    {
        public override ESpellType SpellType => ESpellType.Counter;

        [Header("Counter")]
        [Description("Spell Caster when the counter procs")]
        public ECounterType CounterType;
        public ECounterActivation CounterActivation;
        public bool IsBlockingMovement  = true;
        public bool IsBlockingCast      = true;

        [Description("Spell Casted when the counter procs")]
        [ConditionalField("CounterType", false, ECounterType.Proc)]
        public ESpell OnCounterProc;

        [Header("Counter Extra Graphics")]
        [Description("Change of character color")]
        public Color ColorSwap;
    }
}