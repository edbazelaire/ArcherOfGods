using Enums;
using Game.Spells;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Data
{
    public enum RuneActivationCondition
    {
        None,
        CombatStart,
        CombatEnd,
        LifeBelow,
    }

    [CreateAssetMenu(fileName = "BuffRune", menuName = "Game/Runes/Buff")]
    public class BuffRune : RuneData
    {
        #region Members

        RuneActivationCondition Activation;

        #endregion


        #region Update

        

        #endregion
    }
}