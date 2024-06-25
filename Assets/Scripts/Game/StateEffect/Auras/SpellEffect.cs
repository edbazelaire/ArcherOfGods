using Data;
using Enums;
using Game.Loaders;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Game.Spells
{
    [CreateAssetMenu(fileName = "SpellEffect", menuName = "Game/StateEffects/SpellEffect")]
    public class SpellEffect : StateEffect
    {
        [Header("Spell Effects")]
        [SerializeField] protected List<SpellData>          m_OnHits;
        [SerializeField] protected List<SStateEffectData>   m_AllyStateEffects;
        [SerializeField] protected List<SStateEffectData>   m_EnemyStateEffects;

        public List<SpellData>  OnHits => m_OnHits;
        public List<SStateEffectData> AllyStateEffects => m_AllyStateEffects;
        public List<SStateEffectData> EnemyStateEffects => m_EnemyStateEffects;
    }
}