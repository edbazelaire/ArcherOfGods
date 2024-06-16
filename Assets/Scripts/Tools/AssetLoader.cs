﻿using Data.GameManagement;
using Enums;
using Menu.Common.Buttons;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Tools
{
    public static class AssetLoader
    {
        const string c_IconPrefix                           = "Ic_";
        const string c_ChestSuffix                          = "Chest";
        const string c_TemplatePrefix                       = "Template";

        // =============================================================================================================
        // DATA
        public const string c_DataPath                      = "Data/";
        public const string c_CharacterDataPath             = c_DataPath + "Characters/";
        public const string c_SpellDataPath                 = c_DataPath + "Spells/";
        public const string c_StateEffectDataPath           = c_DataPath + "StateEffects/";
        public const string c_ItemsDataPath                 = c_DataPath + "Items/";
        public const string c_AchievementsDataPath          = c_DataPath + "Achievements/";
        public const string c_ChestsDataPath                = c_ItemsDataPath + "Chests/";
        public const string c_ManagementDataPath            = c_DataPath + "GameManagement/";
        public const string c_ArenaDataPath                 = c_ManagementDataPath + "Arenas/";
        public const string c_AIDataPath                    = c_DataPath + "AI/";

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
        public const string c_TemplatesShopPath             = c_TemplatesUIPath + "Shop/";
        public const string c_AchievementsTemplatesPath     = c_TemplatesUIPath + "Achievements/";
        // ---- Commons
        public const string c_CommonPath                    = c_UIPath + "Common/";
        public const string c_ButtonPath                    = c_CommonPath + "Buttons/";
        // ---- Menus
        public const string c_MainUIPath                    = c_UIPath + "Main/";
        public const string c_MainUIComponentsPath          = c_MainUIPath + "Components/";
        public const string c_MainUIComponentsInfosPath     = c_MainUIComponentsPath + "Infos/";
        public const string c_MainMenuPath                  = c_MainUIPath + "MainMenu/";
        public const string c_MainTabPath                   = c_MainMenuPath + "MainTab/";
        public const string c_ProfileTabPath                = c_MainMenuPath + "ProfileTab/";
        // ---- solo mode ui
        public const string c_ArenaModeUIPath               = c_MainTabPath + "GameSection/ArenaMode/";
        public const string c_RankedModeUIPath              = c_MainTabPath + "GameSection/RankedMode/";
        // ---- settings data
        public const string c_SettingsPath                  = c_UIPath + "Settings/";
        // ---- PopUps & Overlays
        public const string c_OverlayPath                   = c_UIPath + "OverlayScreens/";
        public const string c_PopUpsPath                    = c_OverlayPath + "PopUps/";

        // =============================================================================================================
        // SPRITES
        public const string c_SpritesPath                   = "Sprites/";
        
        // -- UI
        public const string c_UISpritesPath                 = c_SpritesPath + "UI/";
        public const string c_RaysPath                      = c_UISpritesPath + "Rays/";
        
        // -- Backgrounds
        public const string c_BackgroundsPath               = c_SpritesPath + "Backgrounds/";
        public const string c_ArenaBackgroundsPath          = c_BackgroundsPath + "Arenas/";

        // -- Leagues
        public const string c_LeagueBannersPath             = c_SpritesPath + "Leagues/";

        // -- Profile
        public const string c_ProfilePath                   = "Sprites/Profile/";
        public const string c_AvatarsPath                   = c_ProfilePath + "Avatars/";
        public const string c_BordersPath                   = c_ProfilePath + "Borders/";
        public const string c_BadgesPath                    = c_ProfilePath + "Badges/";
        
        // -- Icons
        public const string c_IconPath                      = "Sprites/Icons/";
        public const string c_IconCharactersPath            = c_IconPath + "Characters/";
        public const string c_IconSpellsPath                = c_IconPath + "Spells/";
        public const string c_IconRunesPath                 = c_IconPath + "Runes/";
        public const string c_IconStateEffectsPath          = c_IconPath + "StateEffects/";
        public const string c_ItemsPath                     = c_IconPath + "Items/";
        public const string c_CurrenciesPath                = c_ItemsPath + "Currencies/";
        public const string c_ChestsIconPath                = c_ItemsPath + "Chests/";
        public const string c_ShopPath                      = c_IconPath + "Shop/";
        public const string c_IconUIElementsPath            = c_IconPath + "UIElements/";

        // =============================================================================================================
        // ANIMATIONS
        public const string c_AnimationPath                 = "Animations/";
        public const string c_AnimationParticlesPath        = c_AnimationPath + "Particles/";
        public const string c_AnimationBackgroundsPath      = c_AnimationPath + "Backgrounds/";

        // SOUNDS
        public const string c_SoundsPath                = "Sounds/";


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
            var ressource = Load<T>(dirpath + assetName);
            if (ressource != null)
                return ressource;

            var allRessources = LoadAll<T>(dirpath);
            foreach(var temp in allRessources)
            {
                if (temp.name == assetName)
                {
                    return temp;
                }
            }

            ErrorHandler.Error("Unable to find any ressource named " + assetName + " with type " + typeof(T) + " at " + dirpath);
            return null;
        }

        public static T[] LoadAll<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }

        #endregion


        #region Data Loading

        public static ArenaData LoadArenaData(EArenaType arena)
        {
            return Load<ArenaData>(arena.ToString(), c_ArenaDataPath);
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

        public static GameObject LoadChestPrefab(EChest chestType)
        {
            return Load<GameObject>(c_ItemsPrefabPath + chestType.ToString() + c_ChestSuffix);
        }

        public static GameObject LoadTemplateItem(string suffix)
        {
            return Load<GameObject>(c_TemplatesUIPath + c_TemplatePrefix + suffix);
        }

        public static T LoadTemplateItem<T>(string suffix = "") where T : Object
        {
            if (suffix != "")
                return Load<T>(c_TemplatesUIPath + c_TemplatePrefix + suffix);

            var allTemplates = LoadAll<T>(c_TemplatesUIPath);
            
            if (allTemplates.Length == 0)
            {
                ErrorHandler.Warning("Unable to find any template of type : " + typeof(T).ToString() + " - in " + c_TemplatesUIPath);
                return null;
            }

            if (allTemplates.Length > 1)
            {
                ErrorHandler.Warning("Found multiple templates with type : " + typeof(T).ToString());
            }

            return allTemplates[0];
        }

        public static GameObject LoadTemplateItem(System.Enum item)
        {
            return LoadTemplateItem(item.GetType().ToString().Split('.')[1][1..] + "Item");
        }

        public static TemplateShopItemUI LoadShopTemplateItem(string name = "")
        {
            if (name == "" || name == "default")
            {
                name = "Shop";
            }

            return Load<TemplateShopItemUI>(c_TemplatesShopPath + c_TemplatePrefix + name + "Item");
        }

        public static T LoadShopTemplateItem<T>(string name = "") where T : TemplateShopItemUI
        {
            var allTemplates = LoadAll<T>(c_TemplatesShopPath);

            if (allTemplates.Length == 0)
            {
                ErrorHandler.Warning("Unable to find any template of type : " + typeof(T).ToString() + " - in " + c_TemplatesUIPath);
                return null;
            }

            if (allTemplates.Length > 1)
            {
                if (name != "")
                {
                    foreach (var template in allTemplates)
                    {
                        if (template.name == name)
                            return template;
                    }

                    ErrorHandler.Warning("Found multiple templates with type : " + typeof(T).ToString() + " - but none was named : " + name);
                }
                else
                {
                    ErrorHandler.Warning("Found multiple templates with type : " + typeof(T).ToString() + " - but no name was provided to distinguish");
                }
            }

            return allTemplates[0];
        }

        public static AchievementRewardUI LoadAchievementRewardTemplate(EAchievementReward achievementReward)
        {
            string templateBaseName = achievementReward.ToString();
            if (achievementReward == EAchievementReward.Border)
            {
                templateBaseName = EAchievementReward.Avatar.ToString();
            }
            return Load<AchievementRewardUI>(templateBaseName + "Button", c_AchievementsTemplatesPath);
        }

        public static GameObject LoadArenaButton(EArenaType arenaType)
        {
            return Load<GameObject>(arenaType.ToString() + "Button", c_ArenaModeUIPath + "ArenaButtons/");
        }

        public static LeagueBannerButton LoadLeagueButton()
        {
            return Load<LeagueBannerButton>("LeagueBannerButton", c_RankedModeUIPath);
        }

        #endregion


        #region Icon Loading

        public static Sprite LoadIcon (string itemName, System.Type iconType)
        {
            string path;
            
            if (iconType == typeof(ECharacter))
                path = c_IconCharactersPath;

            else if (iconType == typeof(ESpell))
                path = c_IconSpellsPath;

            else if (iconType == typeof(EStateEffect))
                path = c_IconStateEffectsPath;

            else if (iconType == typeof(ERune))
                path = c_IconRunesPath;

            else if (iconType == typeof(ECurrency))
                path = c_CurrenciesPath;

            else if (iconType == typeof(EAvatar))
                return Load<Sprite>(itemName, AssetLoader.c_AvatarsPath);

            else if (iconType == typeof(EBorder))
                return Load<Sprite>(itemName, AssetLoader.c_BordersPath);

            else if (iconType == typeof(EBadge))
                return Load<Sprite>(itemName, AssetLoader.c_BadgesPath);

            else if (iconType == typeof(EChest))
            {
                path = c_ChestsIconPath;
                itemName += "Chest";
            }
            else
            {
                ErrorHandler.Error("Unhandled type of enum " + iconType + " for icon " + itemName + " - skipping");
                return null;
            }
            return Load<Sprite>(path + c_IconPrefix + itemName);
        }

        public static Sprite LoadIcon(System.Enum value)
        {
            return LoadIcon(value.ToString(), value.GetType());
        }

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
            return Load<Sprite>(c_IconRunesPath + c_IconPrefix + rune.ToString());
        }

        public static Sprite LoadCurrencyIcon(ECurrency currency, int? qty = null)
        {
            if (!qty.HasValue || currency == ECurrency.Xp)
                return Load<Sprite>(c_CurrenciesPath + c_IconPrefix + currency.ToString());
           
            float factor = currency == ECurrency.Gems ? 500f : 5000f;
            int packNumber = Mathf.Clamp((int)Mathf.Round(qty.Value / factor), 1, currency == ECurrency.Golds ? 4 : 3);
            return Load<Sprite>(c_ShopPath + currency.ToString() + "Pack_0" + packNumber.ToString());
        }

        public static Sprite LoadShopIcon(string shopOfferName)
        {
            return Load<Sprite>(c_ShopPath + c_IconPrefix + shopOfferName);
        }

        public static Sprite LoadChestIcon(string chest)
        {
            return Load<Sprite>(c_ChestsIconPath + c_IconPrefix + chest.ToString() + c_ChestSuffix);
        }

        public static Sprite LoadChestIcon(EChest chest)
        {
            return LoadChestIcon(chest.ToString());
        }

        public static Sprite LoadLeagueBanner(ELeague league)
        {
            return Load<Sprite>(c_LeagueBannersPath + league.ToString() + "LeagueBanner");
        }

        public static Sprite LoadAchievementRewardIcon(string name, EAchievementReward arType)
        {
            string path;
            switch (arType)
            {
                case EAchievementReward.Avatar:
                    path = c_AvatarsPath;
                    break;
                case EAchievementReward.Border:
                    path = c_BordersPath;
                    break;
                case EAchievementReward.Badge:
                    path = c_BadgesPath;
                    break;

                default:
                    ErrorHandler.Error("Unhandled case " + arType);
                    return default;
            }

            return AssetLoader.Load<Sprite>(name, path);
        }

        public static Sprite LoadUIElementIcon(string name)
        {
            return Load<Sprite>(c_IconUIElementsPath + c_IconPrefix + name);
        }

        #endregion


        #region Profile Loading

        public static Sprite LoadBadgeIcon(EBadge badge, ELeague league)
        {
            return LoadBadgeIcon(badge.ToString() + (league != ELeague.None ? league.ToString() : ""));
        }

        public static Sprite LoadBadgeIcon(string badge)
        {
            return Load<Sprite>(badge, c_BadgesPath);
        }

        #endregion


        #region Animations

        public static GameObject LoadBackgroundAnimation(string name)
        {
            return Load<GameObject>(name, c_AnimationBackgroundsPath);
        }

        #endregion
    }
}