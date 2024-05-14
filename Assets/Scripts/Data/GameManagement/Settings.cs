using Game;
using Tools;
using UnityEngine;

namespace Data.GameManagement
{
    public enum ESettings
    {
        GlobalSpeed,
        CharacterSizeFactor,
        CharacterSpeedFactor,
        SpellSizeFactor,
        SpellSpeedFactor,
    }

    [CreateAssetMenu(fileName = "Settings", menuName = "Game/Management/Settings")]
    public class Settings : ScriptableObject
    {
        #region Members

        // GAME Speed & Size
        [Header("Game Speed & Size")]
        [SerializeField] float m_GlobalSpeed;
        [SerializeField] float m_CharacterSizeFactor;
        [SerializeField] float m_CharacterSpeedFactor;
        [SerializeField] float m_SpellSizeFactor;
        [SerializeField] float m_SpellSpeedFactor;

        // Level Up Management
        [Header("Level Management")]
        [SerializeField] float m_SpellScaleFactor;

        public static float CharacterSizeFactor     { get => Instance.m_CharacterSizeFactor;    set => Instance.m_CharacterSizeFactor = value; }
        public static float CharacterSpeedFactor    { get => Instance.m_CharacterSpeedFactor;   set => Instance.m_CharacterSpeedFactor = value; }
        public static float SpellSizeFactor         { get => Instance.m_SpellSizeFactor;        set => Instance.m_SpellSizeFactor = value; }
        public static float SpellSpeedFactor        { get => Instance.m_SpellSpeedFactor;       set => Instance.m_SpellSpeedFactor = value; }
        public static float SpellScaleFactor        { get => Instance.m_SpellScaleFactor; }

        #endregion


        #region Instance

        static Settings s_Instance;

        public static Settings Instance
        {

            get
            {
                if (s_Instance == null)
                {
                    s_Instance = AssetLoader.Load<Settings>("Settings", AssetLoader.c_ManagementDataPath);
                }

                return s_Instance;
            }
        }

        #endregion


        #region Getters & Setters

        public static float Get(ESettings setting)
        {
            switch (setting)
            {
                case ESettings.GlobalSpeed:
                    return Instance.m_GlobalSpeed;
                case ESettings.CharacterSizeFactor:
                    return Instance.m_CharacterSizeFactor;
                case ESettings.CharacterSpeedFactor:
                    return Instance.m_CharacterSpeedFactor;
                case ESettings.SpellSizeFactor:
                    return Instance.m_SpellSizeFactor;
                case ESettings.SpellSpeedFactor:
                    return Instance.m_SpellSpeedFactor;

                default:
                    ErrorHandler.Error("Unhandled setting : " + setting);
                    return 1f;
            }
        }

        public static void Set(ESettings setting, float value)
        {
            if (value <= 0)
            {
                ErrorHandler.Error("Trying to set " + setting + " with a value <= 0 : " + value);
            }
            switch (setting)
            {
                case ESettings.GlobalSpeed:
                    Instance.m_GlobalSpeed = value;
                    Time.timeScale = value;
                    return;

                case ESettings.CharacterSizeFactor:
                    Instance.m_CharacterSizeFactor = value;
                    if (GameManager.Instance == null || ! GameManager.Instance.IsGameStarted)
                        return;

                    GameManager.Instance.RescaleCharacters();
                    return;

                case ESettings.CharacterSpeedFactor:
                    Instance.m_CharacterSpeedFactor = value;
                    return;
                
                case ESettings.SpellSizeFactor:
                    Instance.m_SpellSizeFactor = value;
                    return;
                
                case ESettings.SpellSpeedFactor:
                    Instance.m_SpellSpeedFactor = value;
                    return;

                default:
                    ErrorHandler.Error("Unhandled setting : " + setting);
                    return;
            }
        }

        #endregion
    }
}