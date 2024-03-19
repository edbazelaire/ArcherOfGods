using Enums;
using UnityEditor;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Jump", menuName = "Game/Spells/Jump")]
    public class JumpData : ProjectileData
    {
        public override ESpellType SpellType => ESpellType.Jump;

        [Header("Jump Data")]
        public EJumpType JumpType = EJumpType.None;
    }
}