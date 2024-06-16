using Enums;
using System;
using UnityEngine;

namespace Tools
{
    public enum EPlayerPref
    {
        PlayerName,
        GameMode,
        ArenaType,
        WarningMessageAccepted,
    }

    public enum EDebugOption
    {
        Console,
        Monitor,
        ErrorHandler,
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
                PlayerPrefs.SetString(EPlayerPref.GameMode.ToString(), EGameMode.Arena.ToString());

            if (!Enum.TryParse(PlayerPrefs.GetString(EPlayerPref.ArenaType.ToString()), out EArenaType arena))
                SetArenaType(EArenaType.FireArena);

            if (PlayerPrefs.GetInt(EPlayerPref.WarningMessageAccepted.ToString()) == 0)
                SetWarningAccepted(false);
        }


        #endregion


        #region Getters & Setters

        public static EGameMode GetGameMode()
        {
            if (!Enum.TryParse(PlayerPrefs.GetString(EPlayerPref.GameMode.ToString()), out EGameMode gameMode))
            {
                ErrorHandler.Error("Unable to parse game mode : " + PlayerPrefs.GetString(EPlayerPref.GameMode.ToString()));
                gameMode = EGameMode.Arena;
                SetGameMode(EGameMode.Arena);
            }

            return gameMode;
        }

        public static void SetGameMode(EGameMode gameMode)
        {
            PlayerPrefs.SetString(EPlayerPref.GameMode.ToString(), gameMode.ToString());
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
            PlayerPrefs.SetString(EPlayerPref.ArenaType.ToString(), arenaType.ToString());
            PlayerPrefs.Save();

            ArenaTypeChangedEvent?.Invoke(arenaType);
        }

        public static void SetVolume(EVolumeOption volume, float value)
        {
            if (value < 0 || value > 1)
            {
                ErrorHandler.Error("Bad volume provided : " + value);
                value = Mathf.Clamp(value, 0, 1);
            }

            PlayerPrefs.SetFloat(volume.ToString(), value);
            PlayerPrefs.Save();
        }

        public static float GetVolume(EVolumeOption volume)
        {
            return PlayerPrefs.GetFloat(volume.ToString(), 1f);
        }

        public static void SetMuted(EVolumeOption volume, int muted)
        {
            PlayerPrefs.SetInt(volume.ToString() + "_Muted", muted);
            PlayerPrefs.Save();
        }

        public static bool GetMuted(EVolumeOption volume)
        {
            return PlayerPrefs.GetInt(volume.ToString() + "_Muted", 0) == 1;
        }

        public static void SetWarningAccepted(bool accepted)
        {
            PlayerPrefs.SetInt(EPlayerPref.WarningMessageAccepted.ToString(), accepted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool GetWarningAccepted()
        {
            return PlayerPrefs.GetInt(EPlayerPref.WarningMessageAccepted.ToString(), 0) > 0;
        }

        public static void SetDebug(EDebugOption option, bool activate)
        {
            PlayerPrefs.SetInt(option.ToString(), activate ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool GetDebug(EDebugOption option)
        {
            return PlayerPrefs.GetInt(option.ToString(), option == EDebugOption.ErrorHandler ? 1 : 0) == 1;
        }

        #endregion
    }
}