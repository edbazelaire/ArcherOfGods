using Enums;
using System;

namespace Save.Data
{
    [Serializable]
    public class SInGameEventCloudData : SAnalyticsData
    {
        public EGameMode    GameMode;
        public ECharacter   Character;
        public string       Spell;
        public string       HitType;

        public SInGameEventCloudData(EGameMode gameMode, ECharacter character, string spell, string hitType, int qty) : base()
        {
            GameMode    = gameMode;
            Character   = character;
            Spell       = spell;
            HitType     = hitType;
            Count       = qty;
        }

        public override object GetValue(EAnalyticsParam analyticsParam, bool throwError = true)
        {
            switch (analyticsParam)
            {
                case EAnalyticsParam.GameMode:
                    return GameMode;

                case EAnalyticsParam.Character:
                    return Character;

                case EAnalyticsParam.Spell:
                    return Spell;

                case EAnalyticsParam.HitType:
                    return HitType;

                default:
                    return base.GetValue(analyticsParam, throwError);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SInGameEventCloudData other)
            {
                return this.GameMode == other.GameMode
                       && this.Character == other.Character
                       && this.Spell == other.Spell
                       && this.HitType == other.HitType;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GameMode, Character, Spell, HitType);
        }
    }
}