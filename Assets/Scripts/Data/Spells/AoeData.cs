using Enums;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Aoe", menuName = "Game/Spells/Aoe")]
    public class AoeData : SpellData
    {
        public override ESpellType SpellType => ESpellType.Aoe;
    }
}