using Enums;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.ComponentModel;
using Tools;
using Inventory;

namespace Data.GameManagement
{
    [Serializable]
    public struct SPriceData
    {
        public int Price;
        public ECurrency Currency;

        public SPriceData(int price, ECurrency currency)
        {
            Price = price;
            Currency = currency;
        }
    }

    [Serializable]
    public struct SCollectableReward
    {
        public ECollectableType CollectableType;
        public string           CollectableName;
        public int              Qty;

        public SCollectableReward(ECollectableType CollectableType, string CollectableName, int Qty)
        {
            this.CollectableType = CollectableType;
            this.CollectableName = CollectableName;
            this.Qty = Qty;
        }
    }

    [Serializable]
    public struct SCurrencyReward
    {
        public ECurrency    Currency;
        public int          Qty;

        public SCurrencyReward(ECurrency currency, int qty)
        {
            this.Currency = currency;
            this.Qty = qty;
        }
    }

    [Serializable]
    public struct SRewardsData
    {
        public List<SCurrencyReward>    Currencies;
        public List<EChest>             Chests;
        public List<SCollectableReward> Collectables;

        public readonly bool IsEmpty => Currencies.Count + Chests.Count + Collectables.Count == 0;

        public SRewardsData(List<SCurrencyReward> currencyRewards = default, List<EChest> chests = default, List<SCollectableReward> collectableRewards = default)
        {
            Currencies      = currencyRewards != default ? currencyRewards : new List<SCurrencyReward>();
            Chests          = chests != default ? chests : new List<EChest>();
            Collectables    = collectableRewards != default ? collectableRewards : new List<SCollectableReward>();
        }

        public void Add(Enum item, int qty)
        {
            if (item.GetType() == typeof(ECurrency))
            {
                Currencies ??= new List<SCurrencyReward>();
                Currencies.Add(new SCurrencyReward((ECurrency)item, qty));
            } 
            
            else if (item.GetType() == typeof(EChest))
            {
                Chests ??= new List<EChest>();
                Chests.Add((EChest)item);
            } 
            
            else if (CollectablesManagementData.TryGetCollectableType(item, out ECollectableType collectableType))
            {
                Collectables ??= new List<SCollectableReward>();
                Collectables.Add(new SCollectableReward(collectableType, item.ToString(), qty));
            } 

            else
                ErrorHandler.Error("Unhandled type of item " + item.GetType());

        }

        public List<SReward> Rewards
        {
            get
            {
                List<SReward> list = AsRewardStruct(Currencies);
                list.AddRange(AsRewardStruct(Chests));
                list.AddRange(AsRewardStruct(Collectables));

                return list;
            }
        }

        public List<SReward> AsRewardStruct(List<SCurrencyReward> currencies)
        {
            if (currencies == null || currencies.Count == 0)
                return new List<SReward>();

            var rewards = new List<SReward>();
            foreach (SCurrencyReward data in currencies)
            {
                rewards.Add(new SReward(typeof(ECurrency), data.Currency.ToString(), data.Qty));
            }

            return rewards;
        }

        public List<SReward> AsRewardStruct(List<EChest> chests)
        {
            if (chests == null || chests.Count == 0)
                return new List<SReward>();

            var rewards = new List<SReward>();
            foreach (EChest data in chests)
            {
                rewards.Add(new SReward(typeof(EChest), data.ToString(), 1));
            }

            return rewards;
        }

        public List<SReward> AsRewardStruct(List<SCollectableReward> collectables)
        {
            if (collectables == null || collectables.Count == 0)
                return new List<SReward>();

            var rewards = new List<SReward>();
            foreach (SCollectableReward data in collectables)
            {
                rewards.Add(new SReward(CollectablesManagementData.GetEnumType(data.CollectableType), data.CollectableName, data.Qty));
            }

            return rewards;
        }
    }

    [Serializable]
    public struct SRaretyPriceData
    {
        public ERarety      Rarety;
        public SPriceData       Price;    

        public SRaretyPriceData(ERarety rarety, SPriceData price)
        {
            Rarety      = rarety;
            Price       = price;
        }
    }

    [Serializable]
    public struct SShopData
    {
        public string Name;
        /// <summary> list of chests in this bundle </summary>
        public Sprite Icon;
        /// <summary> list of chests in this bundle </summary>
        public SRewardsData Rewards;
        /// <summary> currency used to pay for this bundle </summary>
        public ECurrency Currency;
        /// <summary> amount of necessary currency required </summary>
        public float Cost;

        public SShopData(string name, Sprite icon, SRewardsData rewards, ECurrency currency, int cost)
        {
            Name        = name;
            Icon        = icon;
            Rewards     = rewards;
            Currency    = currency;
            Cost        = cost;
        }
    }

    [Serializable]
    public struct SCurrencyColor
    {
        public ECurrency Currency;
        public Color Color;
    }

    [CreateAssetMenu(fileName = "ShopManagementData", menuName = "Game/Management/Shop")]
    public class ShopManagementData : ScriptableObject
    {
        #region Members

        // ===============================================================================
        // CONFIG
        [Header("Configuration")]
        [Description("color code of currencies")]
        [SerializeField] private List<SCurrencyColor> m_CurrencyColors;

        [Header("Shop Offers")]
        [Description("list of each chests offers in the shop")]
        [SerializeField] private List<SShopData> m_BundleShopData;

        [Description("list of all golds pack offers")]
        [SerializeField] private List<SShopData> m_GoldsShopData;

        [Description("list of each gems offers in the shop")]
        [SerializeField] private List<SShopData> m_GemsShopData;

        [Description("list of price of each characters")]
        [SerializeField] private List<SRaretyPriceData> m_CharacterPrices;

        // ===============================================================================
        // Static Accessors
        private static ShopManagementData m_Instance;

        public static List<SCurrencyColor> CurrencyColors   => Instance.m_CurrencyColors;
        public static List<SShopData> BundleShopData        => Instance.m_BundleShopData;
        public static List<SShopData> GoldsShopData         => Instance.m_GoldsShopData;
        public static List<SShopData> GemsShopData          => Instance.m_GemsShopData;

        #endregion


        #region Accessors

        public static ShopManagementData Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = AssetLoader.Load<ShopManagementData>("ShopManagementData", AssetLoader.c_ManagementDataPath);
                }

                return m_Instance;
            }
        }

        public static Color GetCurrencyColor(ECurrency currency)
        {
            foreach (SCurrencyColor currencyColor in CurrencyColors)
            {
                if (currencyColor.Currency == currency)
                    return currencyColor.Color;
            }

            ErrorHandler.Error("Unable to find CurrencyColor config for currency " + currency);
            return Color.white;
        }

        public static SPriceData GetPrice(Enum collectable)
        {
            ERarety rarety = CollectablesManagementData.GetData(collectable, 1).Rarety;
            foreach (var data in Instance.m_CharacterPrices)
            {
                if (data.Rarety == rarety)
                    return data.Price;
            }

            ErrorHandler.Error("No price data found for collectable " + collectable + " of rarety " + rarety);
            return new SPriceData(0, ECurrency.Golds);
        }

        #endregion
    }
}