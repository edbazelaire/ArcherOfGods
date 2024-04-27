using Enums;
using Save;
using Save.Data;
using Unity.Services.Analytics;

namespace Analytics.Events
{
    public class GameEndedEvent : Event
    {
        protected static EAnalytics EventType => EAnalytics.GameEnded;

        // Constructor for GameEnded event
        public GameEndedEvent(EGameMode gameMode, bool win, ECharacter character, int playerLevel) : base(EventType.ToString())
        {
            // Set event parameters using SetParameter method
            SetParameter(EAnalyticsParam.GameMode.ToString(), gameMode.ToString());
            SetParameter(EAnalyticsParam.Win.ToString(), win);
            SetParameter(EAnalyticsParam.Character.ToString(), character.ToString());
            SetParameter(EAnalyticsParam.CharacterLevel.ToString(), playerLevel);

            StatCloudData.AddAnalytics(EventType, new SGameEndedCloudData(gameMode, win, character));
        }
    }
}

