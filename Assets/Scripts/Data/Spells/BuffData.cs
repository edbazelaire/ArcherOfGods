using Enums;
using Game.Managers;
using Game.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Game/Spells/Buff")]
    public class BuffData : SpellData
    {
        public override ESpellType SpellType => ESpellType.Buff;


        #region State Effect

        public StateEffect GetStateEffect()
        {
            return SpellLoader.GetStateEffect(SpellName, Level);
        }

        #endregion


        #region Infos

        public override Dictionary<string, object> GetInfos()
        {
            var infoDict = base.GetInfos();
            var stateEffectInfos = GetStateEffect().GetInfos();

            foreach (var item in stateEffectInfos)
            {
                if (infoDict.ContainsKey(item.Key))
                    infoDict[item.Key] = item.Value; 
                else 
                    infoDict.Add(item.Key, item.Value);
            }
            
            return infoDict;
        }

        #endregion
    }
}