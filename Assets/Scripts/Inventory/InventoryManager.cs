using Assets;
using Data.GameManagement;
using Enums;
using Game.Managers;
using Save;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace Inventory
{
    public static class InventoryManager
    {
        #region Members

        // =================================================================================================
        // CONSTANTS
        public const        int                     MAX_CHESTS      = 4;
        public static       ERewardType[]           CURRENCIES      => new ERewardType[] { ERewardType.Golds };

        // =================================================================================================
        // EVENTS
        public static       Action<ChestData, int>  ChestsAddedEvent;
        public static       Action<ESpell>          UnlockSpellEvent;
        public static       Action<ECharacter>      CharacterLeveledUpEvent;
        public static       Action<ECharacter, int> CharacterGainedXpEvent;

        // =================================================================================================
        // ACCESSORS
        public static       List<SSpellCloudData>   SpellData       => (List<SSpellCloudData>)InventoryCloudData.Instance.Data[ERewardType.Spell.ToString()];
        public static       int                     Golds           => Convert.ToInt32(InventoryCloudData.Instance.Data[InventoryCloudData.KEY_GOLDS]);
        public static       ChestData[]             Chests          => (ChestData[])ChestsCloudData.Instance.Data[ChestsCloudData.KEY_CHESTS];

        #endregion


        #region Currency Management

        public static int GetCurrency(ERewardType rewardType)
        {
            if (! CURRENCIES.Contains(rewardType))
            {
                ErrorHandler.Error("Reward " + rewardType + " is not a currency");
                return 0;
            }

            return (int)InventoryCloudData.Instance.Data[rewardType.ToString()];
        }

        public static void UpdateCurrency(ERewardType reward, int amount) 
        {
            var data = InventoryCloudData.Instance.Data;

            if (!data.ContainsKey(reward.ToString()))
            {
                ErrorHandler.Error("Currency " + reward + " not found in inventory cloud data");
                return;
            }
            
            var total = (int)data[reward.ToString()] + amount;
            if (amount < 0)
            {
                ErrorHandler.Error($"Not enought {reward} ({(int)data[reward.ToString()]}) to spend ({amount})");
                return;
            }

            InventoryCloudData.Instance.SetData(reward, total);
        }

        /// <summary>
        /// Check if the cost of the item is inf to current amount of golds
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public static bool CanBuy(int cost)
        {
            return Golds - cost > 0;
        }

        /// <summary>
        /// Add golds to cloud data
        /// </summary>
        /// <param name="qty"></param>
        public static void AddGolds(int qty)
        {
            if (qty < 0)
            {
                ErrorHandler.Error($"Trying to add a negative amount of gold ({qty}) : use the Spend() method");
                return;
            }

            InventoryCloudData.Instance.SetData(ERewardType.Golds, Golds + qty);
        }

        /// <summary>
        /// Spend golds
        /// </summary>
        /// <param name="cost"></param>
        public static void SpendGolds(int cost)
        {
            if (cost < 0)
            {
                ErrorHandler.Error($"Trying to add a spend a negative amount of gold ({cost}) : use the AddGolds() method");
                return;
            }

            if (! CanBuy(cost)) 
            {
                ErrorHandler.Error($"Not enought golds ({Golds}) to buy the item ({cost}) : this situation should not happen");
                return;
            }

            InventoryCloudData.Instance.SetData(ERewardType.Golds, Golds - cost);
        }

        #endregion


        #region Spell Management

        /// <summary>
        /// Get data of a spell from cloud data
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public static SSpellCloudData GetSpellData(ESpell spell)
        {
            return InventoryCloudData.Instance.GetSpell(spell);
        }
        
        /// <summary>
        /// Spend cards and golds to update a spell level
        /// </summary>
        /// <param name="spellCloudData"></param>
        public static void Upgrade(SSpellCloudData spellCloudData)
        {
            SSpellLevelData spellLevelData = SpellLoader.GetSpellLevelData(spellCloudData.Spell);
            int requestedQty    = spellLevelData.RequiredCards;
            int requestedGolds  = spellLevelData.RequiredGolds;

            // CHECK:  values before making upgrade
            // -- check requested golds
            if (! CanBuy(requestedGolds))
            {
                ErrorHandler.Error($"Not enought gold ({Golds}) to upgrade spell {spellCloudData.Spell} : requested amount = {spellCloudData.Spell}");
                return;
            }

            // -- check requested quantity
            if (spellCloudData.Qty < requestedQty)
            {
                ErrorHandler.Error($"Not enought cards ({spellCloudData.Qty}) to upgrade spell {spellCloudData.Spell} : requested amount = {requestedQty}");
                return;
            }

            // -- check level
            if (spellCloudData.Level > SpellLoader.SpellsManagementData.SpellLevelData.Count)
            {
                ErrorHandler.Error($"spell {spellCloudData.Spell} is already maxed");
                return;
            }

            // UPGRADE : spend golds and cards to update the level
            SpendGolds(requestedGolds);
            spellCloudData.Qty -= requestedQty;
            spellCloudData.Level++;

            // SAVE : update cloud data
            InventoryCloudData.Instance.SetSpell(spellCloudData);
        }

        /// <summary>
        /// Unlock a spell
        /// </summary>
        /// <param name="spellCloudData"></param>
        public static void Unlock(ref SSpellCloudData spellCloudData)
        {
            if (spellCloudData.Level > 0)
            {
                ErrorHandler.Error("Trying to unlock spell " + spellCloudData.Spell + " but already has level " + spellCloudData.Level);
                return;
            }

            spellCloudData.Level = SpellLoader.GetRaretyData(spellCloudData.Spell).StartLevel;
            InventoryCloudData.Instance.SetSpell(spellCloudData);

            UnlockSpellEvent?.Invoke(spellCloudData.Spell);
        }

        #endregion


        #region Chests Management

        public static bool GetFirstAvailableIndex(out int index)
        {
            index = -1; 
            for (int i = 0; i < Chests.Length; i++)
            {
                if (Chests[i] == null)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }

        public static ChestData CreateRandomChest()
        {
            Array values = Enum.GetValues(typeof(EChestType));
            var random = new System.Random();
            return new ChestData((EChestType)values.GetValue(random.Next(values.Length)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chestType"></param>
        public static void AddChest(EChestType chestType) 
        {
            AddChest(new ChestData(chestType));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chestData"></param>
        public static void AddChest(ChestData chestData)
        {
            if (!GetFirstAvailableIndex(out int index))
                return;

            // add data to list of chest data
            Chests[index] = chestData;

            // fire event that a chest has been added
            ChestsAddedEvent?.Invoke(chestData, index);

            // call for async save of the updated value
            ChestsCloudData.Instance.SaveValue(ChestsCloudData.KEY_CHESTS);
        }

        public static void RemoveChestAtIndex(int index)
        {
            // this can happen if the negative index check was not done before the call
            if (index < 0)
            {
                ErrorHandler.Warning("Trying to remove chest with index " + index + ". This should be handled before calling this method");
                return;
            }

            // this should never happen
            if (index >= Chests.Length)
            {
                ErrorHandler.Error("Trying to remove chest with " + index);
                return;
            }

            // remove chest data from cloud data
            Chests[index] = null;
            ChestsCloudData.Instance.SaveValue(ChestsCloudData.KEY_CHESTS);
        } 

        /// <summary>
        /// Add a list of rewards to the inventory
        /// </summary>
        /// <param name="rewards"></param>
        public static void CollectRewards(List<SReward> rewards)
        {
            foreach (SReward reward in rewards) 
            {
                CollectReward(reward);
            }
        }

        /// <summary>
        /// Add a reward to the inventory
        /// </summary>
        /// <param name="reward"></param>
        public static void CollectReward(SReward reward)
        {
            if (CURRENCIES.Contains(reward.RewardType))
            {
                UpdateCurrency(reward.RewardType, reward.Qty);
                return;
            }

            if (reward.RewardType == ERewardType.Spell)
            {
                var spellCloudData = InventoryCloudData.Instance.GetSpell((ESpell)reward.Metadata[SReward.METADATA_KEY_SPELL_TYPE]);
                if (spellCloudData.Level == 0)
                    Unlock(ref spellCloudData);
                spellCloudData.Qty += reward.Qty;
                InventoryCloudData.Instance.SetSpell(spellCloudData);
                return;
            }

            ErrorHandler.Error("Unhandled case : " + reward.RewardType);
        }


        #endregion


        #region Character Management

        /// <summary>
        /// Add xp to a character : check if raise a level up
        /// </summary>
        /// <param name="character"></param>
        /// <param name="xp"></param>
        public static void AddXp(ECharacter character, int xp)
        {
            if (xp < 0)
            {
                ErrorHandler.Error("Trying to provide negative xp ("+ xp + ") to character " + character);
                return;
            }

            if (xp == 0)
            {
                ErrorHandler.Warning("Trying to provide 0 xp to character " + character);
                return;
            }

            // fire event that character gained xp
            CharacterGainedXpEvent?.Invoke(character, xp);

            // get data of this character (current xp, level)
            SCharacterCloudData charData = InventoryCloudData.Instance.GetCharacter(character);
            
            // get level data (required xp, ...)
            SCharacterLevelData charLevelData = CharacterLoader.CharactersManagementData.GetCharacterLevelData(charData.Level);

            // check that the xp added to the character is below required xp to level up
            charData.Xp += xp;
            if (charData.Xp < charLevelData.RequiredXp)
            {
                // save & return
                InventoryCloudData.Instance.SetCharacter(charData);
                return;
            }

            // retrieve required xp to current character xp
            charData.Xp -= charLevelData.RequiredXp;
            // level up character
            charData.Level += 1;
            // save
            InventoryCloudData.Instance.SetCharacter(charData);

            // raise level upevent
            CharacterLeveledUpEvent?.Invoke(character);

            // call level up popup
            Main.SetPopUp(EPopUpState.LevelUpScreen, character);
        }

        #endregion
    }
}