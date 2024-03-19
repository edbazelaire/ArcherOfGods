using Enums;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Zone", menuName = "Game/Spells/Zone")]
    public class ZoneData : AoeData
    {
        public override ESpellType SpellType => ESpellType.Zone;

        [Header("Zone Data")]
        [Description("Tick over time duration of re-appliance of the spell")]
        public float DurationTick = 0f;
        [Description("Growing factor of the spell (as bonus percentage)")]
        public float GrowSizeFactor = 0f;
        [Description("Percentage of the duration where the Zone will reach max size")]
        public float MaxSizeAt = 1f;
        [Description("List of effects that proc while the player is in the zone")]
        public List<SStateEffectData> PersistentStateEffects;
    }
}