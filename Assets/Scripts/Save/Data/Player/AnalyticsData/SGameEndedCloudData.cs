using Data;
using Enums;
using System;
using System.Collections.Generic;
using Tools;

namespace Save.Data
{
    [Serializable]
    public class SGameEndedCloudData : SAnalyticsData
    {
        public EGameMode GameMode;
        public bool Win;
        public ECharacter ECharacter;

        public SGameEndedCloudData(EGameMode gameMode, bool win, ECharacter character) : base()
        {
            GameMode = gameMode;
            Win = win;
            ECharacter = character;
        }

        public override object GetValue(EAnalyticsParam analyticsParam)
        {
            switch (analyticsParam)
            {
                case EAnalyticsParam.GameMode:
                    return GameMode;

                case EAnalyticsParam.Win:
                    return Win;

                case EAnalyticsParam.Character:
                    return ECharacter;

                default:
                    return base.GetValue(analyticsParam);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SGameEndedCloudData other)
            {
                return this.GameMode == other.GameMode
                       && this.Win == other.Win
                       && this.ECharacter == other.ECharacter;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GameMode, Win, ECharacter);
        }
    }
}