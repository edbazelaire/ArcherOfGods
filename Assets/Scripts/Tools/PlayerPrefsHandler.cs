using Enums;
using System;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public enum EPlayerPref
    {
        PlayerName,
        GameMode,
        ArenaType,
    }

    public static class PlayerPrefsHandler
    {
        #region Members

        public static Action<EGameMode> GameModeChangedEvent;
        public static Action<EArenaType> ArenaTypeChangedEvent;

        #endregion


        #region Init & End

        public static void Initialize()
        {
            if (PlayerPrefs.GetString(EPlayerPref.PlayerName.ToString()) == "")
                PlayerPrefs.SetString(EPlayerPref.PlayerName.ToString(), "SheepRapist");

            if (!Enum.TryParse(PlayerPrefs.GetString(EPlayerPref.GameMode.ToString()), out EGameMode gameMode))
                PlayerPrefs.SetString(EPlayerPref.GameMode.ToString(), EGameMode.Solo.ToString());

            if (!Enum.TryParse(PlayerPrefs.GetString(EPlayerPref.ArenaType.ToString()), out EArenaType arena))
                SetArenaType(EArenaType.FireArena);
        }


        #endregion


        #region Getters & Setters

        public static EGameMode GetGameMode()
        {
            if (!Enum.TryParse(PlayerPrefs.GetString(EPlayerPref.GameMode.ToString()), out EGameMode gameMode))
            {
                ErrorHandler.Error("Unable to parse game mode : " + PlayerPrefs.GetString(EPlayerPref.GameMode.ToString()));
                gameMode = EGameMode.Solo;
                SetGameMode(EGameMode.Solo);
            }

            return gameMode;
        }

        public static void SetGameMode(EGameMode gameMode)
        {
            PlayerPrefs.SetString(EPlayerPref.GameMode.ToString(), EGameMode.Solo.ToString());
            PlayerPrefs.Save();

            GameModeChangedEvent?.Invoke(gameMode);
        }

        public static EArenaType GetArenaType()
        {
            if (!Enum.TryParse(PlayerPrefs.GetString(EPlayerPref.ArenaType.ToString()), out EArenaType arenaType))
            {
                ErrorHandler.Error("Unable to parse solo arena : " + PlayerPrefs.GetString(EPlayerPref.ArenaType.ToString()));
                arenaType = EArenaType.FireArena;
                SetArenaType(arenaType);
            }

            return arenaType;
        }

        public static void SetArenaType(EArenaType arenaType)
        {
            PlayerPrefs.SetString(EPlayerPref.ArenaType.ToString(), EArenaType.FireArena.ToString());
            PlayerPrefs.Save();

            ArenaTypeChangedEvent?.Invoke(arenaType);
        }


        #endregion
    }
}