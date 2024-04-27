using Enums;
using Unity.Services.Analytics;

namespace Analytics.Events
{
    public class SoloGameEndedEvent : GameEndedEvent
    {
        // Constructor for SoloGameEndedEvent
        public SoloGameEndedEvent(bool win, ECharacter character, int playerLevel, int elo, ECharacter enemyCharacter, int enemyLevel) : base(EGameMode.Multi, win, character, playerLevel)
        {
            // Additional parameters specific to SoloGameEndedEvent
            SetParameter("Elo",                 elo);
            SetParameter("EnemyCharacter",      enemyCharacter.ToString());
            SetParameter("EnemyLevel",          enemyLevel);
        }
    }
}

