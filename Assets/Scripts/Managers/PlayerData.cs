using Enums;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Managers
{
    public class PlayerData: MonoBehaviour
    {
        public const string KEY_PLAYER_NAME = "PlayerName";
        public const string KEY_CHARACTER = "Character";
        public const string KEY_CLIENT_ID = "ClientId";

        static PlayerData s_Instance;
        public static PlayerData Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new PlayerData();
                return s_Instance;
            }
        }

        public static string PlayerName;
        public static ECharacter Character;

        public static Dictionary<string, PlayerDataObject> GetPlayerData()
        {
            return new Dictionary<string, PlayerDataObject> {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerData.PlayerName) },
                { KEY_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ((int)PlayerData.Character).ToString()) },
                { KEY_CLIENT_ID, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "") }
            };
        }
    }
}