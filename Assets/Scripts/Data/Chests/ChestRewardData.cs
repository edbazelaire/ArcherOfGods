﻿using Assets.Scripts.Menu.MainMenu.MainTab.Chests;
using Enums;
using Game.Loaders;
using Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Tools;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct SCurrencyDistributionData
    {
        public ECurrency Currency;
        public int Min;
        public int Max;
    }

    [Serializable]
    public struct SRaretyPercData
    {
        [Description("Rarety of the item")]
        public ERarety Rarety;

        [Description("Percentage chances (from 0 to 100) to get this rarety value")]
        public float Percentage;
    }

    [Serializable]
    public struct SExtraCardData
    {
        public int ExtraCards;
        public SRaretyPercData[] RaretyPercData;

        public Dictionary<ERarety, float> ToDict()
        {
            var dict = new Dictionary<ERarety, float>();
            foreach (var data in RaretyPercData)
            {
                // add percentage extra to later generate extra rewards
                if (data.Percentage < 0 || data.Percentage > 100)
                {  
                    ErrorHandler.Error($"Bad PercentageExtra value ({data.Percentage}) for rarety ({data.Rarety})");
                    continue;
                }

                dict.Add(data.Rarety, data.Percentage);
            }

            return dict;
        }
    }

    [Serializable]
    public struct SSpellDistributionData
    {
        [Description("Type of spell that is generated by this distribution")]
        public ERarety Rarety;
        [Description("Number of cards of that rarety")]
        public int Qty;
        [Description("In how many group this quantity is splitten")]
        public int Splits;

        public SSpellDistributionData(ERarety rarety, int qty, int splits = 1)
        {
            Rarety      = rarety;
            Qty         = qty;
            Splits      = splits;
        }

        public List<SReward> Generate(EStateEffect[] stateEffectFilter = default)
        {
            if (Qty == 0)
                return new List<SReward>();

            if (Qty < 0)
            {
                ErrorHandler.Error("Qty < 0 for " + GetType());
                return new List<SReward>();
            }

            // check provided data
            if (Splits <= 0 && Qty > 0)
            {
                ErrorHandler.Error("Splits <= 0 for " + GetType());
                Splits = 1;
            }

            // setup data
            int remaining = Qty;
            List<SReward> rewardsList = new();
            var usedSpells = new List<ESpell>() { };

            if (stateEffectFilter == default)
                stateEffectFilter = new EStateEffect[] {};

            // splits data into N parts
            for (int i = 0; i < Splits; i++)
            {
                int qty;

                // No splits remaning : set qty to remaining data
                if (i >= Splits - 1 || remaining <= Qty / Splits)
                {
                    qty = remaining;
                }
                else
                {
                    // Split the quantity
                    int minAmount = Math.Max(1, Qty / (Splits + 1));
                    int maxAmount = Math.Min(remaining, Qty / (Splits - 1));
                    qty = UnityEngine.Random.Range(minAmount, maxAmount);
                }

                // remove qty from remaining 
                remaining -= qty;

                // generate random quantity of cards and add it to list of rewards
                rewardsList.Add(new SReward(
                    typeof(ESpell),
                    GenerateRandomSpell(Rarety, ref usedSpells, stateEffectFilter.ToList()).ToString(),
                    qty
                ));

                if (remaining == 0)
                    break;
            }

            return rewardsList;
        }

        /// <summary>
        /// Get a random spell from list of spells
        /// </summary>
        /// <param name="rarety"></param>
        /// <param name="stateEffectFilter"></param>
        /// <returns></returns>
        public static ESpell GenerateRandomSpell(ERarety rarety, ref List<ESpell> usedSpells, List<EStateEffect> stateEffectFilter = default)
        {
            var spells = SpellLoader.FilterSpells(
                raretyFilters:          new List<ERarety>() { rarety }, 
                stateEffectFilters:     stateEffectFilter,
                notAllowedSpellsFilter: usedSpells
            );

            // remove unallowed spells if this is bloquant
            if (spells.Count == 0)
            {
                spells = SpellLoader.FilterSpells(
                    raretyFilters: new List<ERarety>() { rarety },
                    stateEffectFilters: stateEffectFilter
                );

                // COUNT still 0 : ERROR
                if (spells.Count == 0)
                {
                    ErrorHandler.Warning("Not able to find any spells with rarety " + rarety + " and state effects : " + TextHandler.ToString(stateEffectFilter));
                    spells = SpellLoader.GetSpellsFromRarety(rarety);
                } 
            } 

            // generate random spell
            var spell = spells[UnityEngine.Random.Range(0, spells.Count)].Spell;

            // add spell to already used spells
            if (usedSpells != null && ! usedSpells.Contains(spell))
                usedSpells.Add(spell);

            return spell;
        }
    }


    [CreateAssetMenu(fileName = "ChestRewardData", menuName = "Game/ChestRewardData")]
    public class ChestRewardData : ScriptableObject
    {
        #region Members

        [Header("Unlocking")]
        public int UnlockTime;
        public AudioClip IdleSoundFX;
        public AudioClip OpenSoundFX;

        [Header("Collectables")]
        [Description("Min/Max Golds from that chest")]
        public SCurrencyDistributionData[] Currencies;
        [Description("Total Number of spells in the chest")]
        public SExtraCardData ExtraCardData;
        [Description("Distrinution of spells in that chest")]
        public SSpellDistributionData[] SpellsDistribution;

        [Header("Filters")]
        [Description("Filter spells to only get spells with provided state effects")]
        public EStateEffect[] StateEffectFilter;

        // ========================================================================================================
        // Dependent Properties
        public EChest ChestType
        {
            get
            {
                string myName = name;
                if (myName.EndsWith("(Clone)"))
                    myName = myName[..^"(Clone)".Length];
                if (myName.EndsWith("Chest"))
                    myName = myName[..^"Chest".Length];

                if (!Enum.TryParse(myName, out EChest chest))
                {
                    ErrorHandler.Error("Unable to parse chest " + name + " into a EChestType");
                    chest = EChest.Common;
                }
                return chest;
            }
        }

        public Sprite Image => AssetLoader.LoadChestIcon(ChestType);

        #endregion


        #region Public Manipulators

        public List<SReward> GenerateRewards()
        {
            List<SReward> rewards = new();

            // CURRENCIES
            foreach (SCurrencyDistributionData data in Currencies)
            {
                if (data.Min < 0 || data.Max < 0 || data.Min > data.Max)
                {
                    ErrorHandler.Error($"Bad min/max ({data.Min}/{data.Max}) data provided for currency ({data.Currency}) in chest " + name);
                    continue;
                }

                rewards.Add(new SReward(typeof(ECurrency), data.Currency.ToString(), UnityEngine.Random.Range(data.Min, data.Max)));
            }

            // SPELLS
            foreach (SSpellDistributionData spellDistribution in SpellsDistribution)
            {
                rewards.AddRange(spellDistribution.Generate(StateEffectFilter));
            }

            // EXTRA CARD
            rewards.AddRange(GenerateExtraCardsRewards(ExtraCardData.ExtraCards, ExtraCardData.ToDict()));

            return rewards;
        }

        List<SReward> GenerateExtraCardsRewards(int extraCardsQty, Dictionary<ERarety, float> extraCardsPerc)
        {
            List<ESpell> usedSpells = default;
            var rewards = new Dictionary<ERarety, SReward>();
            for (int i = 0; i < extraCardsQty; i++)
            {
                var randValue = UnityEngine.Random.Range(0f, 100f);

                foreach (var item in extraCardsPerc.Reverse())
                {
                    if (item.Value < randValue)
                        continue;

                    // rarety exists already : add one more qty
                    if (rewards.ContainsKey(item.Key))
                    {
                        var reward = rewards[item.Key];
                        reward.Qty++;
                        rewards[item.Key] = reward;
                        break;
                    }

                    rewards.Add(item.Key, new SReward(
                        typeof(ESpell),
                        SSpellDistributionData.GenerateRandomSpell(item.Key, ref usedSpells, StateEffectFilter.ToList()).ToString(),
                        1
                    ));
                    break;
                }
            }

            return rewards.Values.ToList();
        } 

        #endregion


        #region Instantiation & GUI Display

        public ChestUI Instantiate(GameObject parent, bool scale = true)
        {
            ChestUI chestUI = GameObject.Instantiate(AssetLoader.LoadChestPrefab(ChestType), parent.transform).GetComponent<ChestUI>();
            chestUI.Initialize();

            // calculate scale of container and update accordingly
            if (scale)
                CoroutineManager.DelayMethod(() => { ScaleOfParent(ref chestUI, parent); });

            // update render value
            UpdateRenderOrder(ref chestUI, parent);

            return chestUI;
        }

        void ScaleOfParent(ref ChestUI chestUI, GameObject parent)
        {
            // get min scaling
            RectTransform parentRectT = Finder.FindComponent<RectTransform>(parent);
            Sprite chestIcon = chestUI.Icon;

            if (chestIcon == null)
                chestIcon = Finder.FindComponent<SpriteRenderer>(chestUI.gameObject).sprite;
            if (chestIcon == null)
                return;

            float minScale = Mathf.Min(parentRectT.rect.width / chestIcon.bounds.size.x, parentRectT.rect.height / chestIcon.bounds.size.y);

            chestUI.transform.localScale = new Vector3(minScale, minScale, 1f);
        }

        /// <summary>
        /// Find base canvas from parent where this game object is instantiated and update the render order to be above
        /// </summary>
        /// <param name="chestUI"></param>
        /// <param name="parent"></param>
        void UpdateRenderOrder(ref ChestUI chestUI, GameObject parent)
        {
            // Find the parent Canvas component
            Canvas canvas = FindParentCanvasComponent(parent.transform);

            // if no canvas : no need to update render order
            if (canvas == null)
                return;

            // Get all SpriteRenderer components in the hierarchy of the Chest object
            var spriteRenderers = Finder.FindComponents<SpriteRenderer>(chestUI.gameObject, throwError: false);

            // Adjust the sorting order of SpriteRenderer components
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.sortingLayerName = canvas.sortingLayerName;
                spriteRenderer.sortingOrder += canvas.sortingOrder + 1; // Render above the Canvas
            }

            // Get all ParticleSystem components in the hierarchy of the Chest object
            var particleSystems = Finder.FindComponents<ParticleSystem>(chestUI.gameObject, throwError: false);

            // Adjust the rendering order of ParticleSystem components
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                Renderer particleRenderer = particleSystem.GetComponent<Renderer>();
                if (particleRenderer != null)
                {
                    particleRenderer.sortingLayerName = canvas.sortingLayerName;
                    particleRenderer.sortingOrder += canvas.sortingOrder + 1; // Render above the Canvas
                }
            }
        }

        Canvas FindParentCanvasComponent(Transform currentTransform)
        {
            // Base case: If current transform is null, return null
            if (currentTransform == null)
            {
                return null;
            }

            // Check if the current transform's parent has a Canvas component
            Canvas parentCanvas = currentTransform.parent?.GetComponent<Canvas>();

            // If parentCanvas is null, recursively search the parent's parent
            if (parentCanvas == null)
            {
                return FindParentCanvasComponent(currentTransform.parent);
            }

            // If parentCanvas is not null, return it
            return parentCanvas;
        }

        #endregion
    }
}