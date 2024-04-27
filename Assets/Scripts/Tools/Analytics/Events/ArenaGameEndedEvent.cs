using Enums;
using Unity.Services.Analytics;

namespace Analytics.Events
{
    public class ArenaGameEndedEvent : GameEndedEvent
    {
        public ArenaGameEndedEvent(bool win, ECharacter character, int playerLevel, EArenaType arenaType, int currentLevel, int currentStage) : base(EGameMode.Solo, win, character, playerLevel) 
        {
            // Additional parameters specific to ArenaGameEndedEvent
            SetParameter("ArenaType", arenaType.ToString());
            SetParameter("CurrentLevel", currentLevel);
            SetParameter("CurrentStage", currentStage);
        }
    }
}

