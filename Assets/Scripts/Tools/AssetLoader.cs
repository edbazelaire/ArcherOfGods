using Enums;
using UnityEngine;

namespace Tools
{
    public static class AssetLoader
    {
        const string c_IconPrefix = "Ic_";

        // =============================================================================================================
        // DATA
        public const string c_DataPath = "Data/";
        public const string c_CharacterDataPath = c_DataPath + "Characters/";
        public const string c_SpellDataPath = c_DataPath + "Spells/";
        public const string c_StateEffectDataPath = c_DataPath + "StateEffects/";
        public const string c_ItemsDataPath = c_DataPath + "Items/";
        public const string c_ChestsDataPath = c_ItemsDataPath + "Chests/";

        // =============================================================================================================
        // PREFABS
        public const string c_PrefabsPath = "Prefabs/";
        
        // SPELLS           -------------------------------------------------------------
        public const string c_SpellsPrefabsPath = c_PrefabsPath + "Spells/";

        // =============================================================================================================
        // UI 
        public const string c_UIPath = "UI/";
        // ---- Commons
        public const string c_CommonPath = c_UIPath + "Common/";
        public const string c_ButtonPath = c_CommonPath + "Buttons/";
        // ---- Menus
        public const string c_MenusPath = c_UIPath + "Menus/";
        public const string c_MainMenuPath = c_MenusPath + "MainMenu/";
        public const string c_MainTab = c_MainMenuPath + "MainTab/";
        // ---- PopUps & Overlays
        public const string c_OverlayPath = c_UIPath + "OverlayScreens/";

        // =============================================================================================================
        // SPRITES
        public const string c_IconPath = "Sprites/Icons/";
        public const string c_IconSpellPath = c_IconPath + "Spells/";
        public const string c_IconStateEffectPath = c_IconPath + "StateEffects/";
        public const string c_ItemPath = c_IconPath + "Items/";
        public const string c_CurrenciesPath = c_ItemPath + "Currencies/";


        #region Default Methods

        public static T Load<T>(string path) where T : Object
        {
            var ressource = Resources.Load<T>(path);
            if (ressource == null)
                ErrorHandler.Warning($"AssetLoader : Ressource {path} not found");
            return ressource;
        }

        public static T Load<T>(string assetName, string dirpath) where T : Object
        {
            return Load<T>(dirpath + assetName);
        }

        public static T[] LoadAll<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }

        #endregion


        #region Spell Prefabs Loading

        public static GameObject[] LoadSpellPrefabs()
        {
            return Resources.LoadAll<GameObject>(c_SpellsPrefabsPath);
        }

        #endregion


        #region Icon Loading

        public static Sprite LoadSpellIcon(ESpell spell)
        {
            return Load<Sprite>(c_IconSpellPath + c_IconPrefix + spell.ToString());
        }

        public static Sprite LoadStateEffectIcon(EStateEffect stateEffect)
        {
            return Load<Sprite>(c_IconStateEffectPath + c_IconPrefix + stateEffect.ToString());
        }

        public static Sprite LoadCurrencyIcon(EReward reward)
        {
            return Load<Sprite>(c_CurrenciesPath + reward.ToString());
        }

        #endregion
    }
}