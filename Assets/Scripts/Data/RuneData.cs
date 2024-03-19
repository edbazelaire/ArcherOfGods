using Enums;
using System.ComponentModel;
using UnityEngine;

namespace Data
{
    public class RuneData : ScriptableObject
    {
        [Description("Description informations of the Rune")]
        public string Description;

        [Description("State effect applying on activation")]
        public EStateEffect StateEffect;
    }
}