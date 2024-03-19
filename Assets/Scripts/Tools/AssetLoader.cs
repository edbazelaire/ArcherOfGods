using Enums;
using UnityEngine;

namespace Tools
{
    public static class AssetLoader
    {
        const string c_IconPrefix                           = "Ic_";
        const string c_ChestPrefix                          = "Chest";
        const string c_TemplatePrefix                       = "Template";

        // =============================================================================================================
        // DATA
        public const string c_DataPath                      = "Data/";
        public const string c_CharacterDataPath             = c_DataPath + "Characters/";
        public const string c_SpellDataPath                 = c_DataPath + "Spells/";
        public const string c_StateEffectDataPath           = c_DataPath + "StateEffects/";
        public const string c_ItemsDataPath                 = c_DataPath + "Items/";
        public const string c_ChestsDataPath                = c_ItemsDataPath + "Chests/";
        public const string c_ManagementDataPath            = c_DataPath + "GameManagement/";

        // =============================================================================================================
        // PREFABS
        public const string c_PrefabsPath                   = "Prefabs/";
        // ---- Characters 
        public const string c_CharactersPreviewPath         = c_PrefabsPath + "Characters/";
        // ---- Spells 
        public const string c_SpellsPrefabsPath             = c_PrefabsPath + "Spells/";
        // ---- Items 
        public const string c_ItemsPrefabPath               = c_PrefabsPath + "Items/";

        // =============================================================================================================
        // UI 
        public const string c_UIPath                        = "UI/";
        // ---- Templates
        public const string c_TemplatesUIPath               = c_UIPath + "Templates/";
        // ---- Commons
        public const string c_CommonPath                    = c_UIPath + "Common/";
        public const string c_ButtonPath                    = c_CommonPath + "Buttons/";
        // ---- Menus
        public const string c_MainUIPath                    = c_UIPath + "Main/";
        public const string c_MainUIComponentsPath          = c_MainUIPath + "Components/";
        public const string c_MainUIComponentsInfosPath     = c_MainUIComponentsPath + "Infos/";
        public const string c_MainMenuPath                  = c_MainUIPath + "MainMenu/";
        public const string c_MainTab                       = c_MainMenuPath + "MainTab/";
        // ---- PopUps & Overlays
        public const string c_OverlayPath                   = c_UIPath + "OverlayScreens/";
        public const string c_PopUpsPath                    = c_OverlayPath + "PopUps/";

        // =============================================================================================================
        // SPRITES
        public const string c_IconPath                      = "Sprites/Icons/";
        public const string c_IconCharactersPath            = c_IconPath + "Characters/";
        public const string c_IconSpellsPath                = c_IconPath + "Spells/";
        public const string c_IconRunesPath                 = c_IconPath + "Runes/";
        public const string c_IconStateEffectsPath          = c_IconPath + "StateEffects/";
        public const string c_ItemsPath                     = c_IconPath + "Items/";
        public const string c_CurrenciesPath                = c_ItemsPath + "Currencies/";
        public const string c_ChestsIconPath                = c_ItemsPath + "Chests/";
        public const string c_IconUIElementsPath            = c_IconPath + "UIElements/";


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


        #region Character & Spells Prefabs Loading

        public static GameObject LoadCharacterPreview(ECharacter character)
        {
            return Load<GameObject>(character.ToString() + "Preview", c_CharactersPreviewPath + character.ToString() + "/");
        }

        public static GameObject[] LoadSpellPrefabs()
        {
            return Resources.LoadAll<GameObject>(c_SpellsPrefabsPath);
        }

        #endregion


        #region ItemUI Prefabs

        public static GameObject LoadChestPrefab(EChestType chestType)
        {
            return Load<GameObject>(c_ItemsPrefabPath + c_ChestPrefix + chestType.ToString());
        }

        public static GameObject LoadTemplateItem(string suffix)
        {
            return Load<GameObject>(c_TemplatesUIPath + c_TemplatePrefix + suffix);
        }

        #endregion


        #region Icon Loading

        public static Sprite LoadCharacterIcon(ECharacter character)
        {
            return Load<Sprite>(c_IconCharactersPath + c_IconPrefix + character.ToString());
        }

        public static Sprite LoadSpellIcon(ESpell spell)
        {
            return Load<Sprite>(c_IconSpellsPath + c_IconPrefix + spell.ToString());
        }

        public static Sprite LoadStateEffectIcon(string stateEffect)
        {
            return Load<Sprite>(c_IconStateEffectsPath + c_IconPrefix + stateEffect);
        }

        public static Sprite LoadRuneIcon(ERune rune)
        {
            return Load<Sprite>(c_IconRunesPath + c_IconPrefix + rune.ToString() + "Rune");
        }

        public static Sprite LoadCurrencyIcon(ERewardType reward)
        {
            return Load<Sprite>(c_CurrenciesPath + reward.ToString());
        }

        public static Sprite LoadChestIcon(EChestType chest)
        {
            return Load<Sprite>(c_ChestsIconPath + c_IconPrefix + chest.ToString() + c_ChestPrefix);
        }

        public static Sprite LoadUIElementIcon(string name)
        {
            return Load<Sprite>(c_IconUIElementsPath + c_IconPrefix + name);
        }

        #endregion
    }
}