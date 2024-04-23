using Enums;
using System.ComponentModel;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "RuneData", menuName = "Game/Runes/Default")]
    public class RuneData : CollectionData
    {
        [Description("Description informations of the Rune")]
        public string Description;

        [Description("State effect applying on activation")]
        public EStateEffect StateEffect;
    }
}