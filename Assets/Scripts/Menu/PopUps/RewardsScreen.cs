using Assets.Scripts.Managers.Sound;
using Assets.Scripts.Menu.Common.Buttons.TemplateItemButtons;
using Assets.Scripts.Menu.MainMenu.MainTab.Chests;
using Data;
using Data.GameManagement;
using Enums;
using Game.Loaders;
using Inventory;
using Menu.Common;
using Menu.Common.Buttons;
using Save;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tools;
using Tools.Animations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class RewardsScreen : OverlayScreen
    {
        #region Members

        const string        c_ChestContainer            = "ChestContainer";
        const string        c_RewardDisplayContainer    = "RewardDisplayContainer";

        GameObject          m_ChestContainer;
        ChestUI             m_ChestUI;
        GameObject          m_RewardDisplayContainer;
        GameObject          m_RewardIconSection;
        GameObject          m_RewardInfosSection;
        TMP_Text            m_RewardTitle;
        CollectionFillBar   m_CollectionFillBar;
        TMP_Text            m_CollectionQty;

        // SPECIFIC DATA
        SRewardsData        m_RewardsData;
        string              m_Context;

        int                 m_Depth = 0;
        GameObject          m_CurrentTemplateItem = null;
        ChestRewardData     m_CurrentChestRewardData    = null;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            // setup game objects
            m_ChestContainer            = Finder.Find(gameObject, c_ChestContainer);

            // Rewars Template
            m_RewardDisplayContainer    = Finder.Find(gameObject, c_RewardDisplayContainer);
            m_RewardIconSection         = Finder.Find(m_RewardDisplayContainer, "RewardIconSection");

            // Infos Section
            m_RewardInfosSection        = Finder.Find(gameObject, "RewardInfosSection");
            m_RewardTitle               = Finder.FindComponent<TMP_Text>(m_RewardInfosSection, "RewardTitle");
            m_CollectionFillBar         = Finder.FindComponent<CollectionFillBar>(m_RewardInfosSection, "CollectionFillbar");
            m_CollectionQty             = Finder.FindComponent<TMP_Text>(m_RewardInfosSection, "CollectionQty");
        }

        /// <summary>
        /// Initialize with a chest
        /// </summary>
        /// <param name="chestType"></param>
        /// <param name="chestIndex"></param>
        public void Initialize(SRewardsData rewardsData, string context)
        {
            m_RewardsData = rewardsData;
            m_Context = context;
            m_CurrentChestRewardData = null;

            base.Initialize();
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            // hide before displaying rewards
            m_RewardDisplayContainer.SetActive(false);
            m_ChestContainer.SetActive(false);
        }

        protected override void OnInitializationCompleted()
        {
            base.OnInitializationCompleted();
            StartCoroutine(StartDisplay());

            SoundFXManager.MusicAudioSource.volume /= 2;
        }

        protected override void EnterAnimation()
        {
            var fadeIn = gameObject.AddComponent<Fade>();
            fadeIn.Initialize("", duration: 0.2f, startOpacity: 0);
        }

        protected override void OnExit()
        {
            base.OnExit();

            SoundFXManager.MusicAudioSource.volume *= 2;
        }

        protected override IEnumerable ExitAnimation()
        {
            var fadeOut = gameObject.AddComponent<Fade>();
            fadeOut.Initialize("", duration: 0.2f, endOpacity: 0);

            yield return new WaitUntil(() => fadeOut.IsOver);
        }

        #endregion


        #region Rewards

        IEnumerator StartDisplay()
        {
            yield return DisplayRewards(m_RewardsData.Rewards);

            Exit();
        }

        IEnumerator DisplayRewards(List<SReward> rewards)
        {
            // depth of the current coroutine 
            int myDepth = m_Depth;

            int i = 0;
            foreach (SReward reward in rewards)
            {
                ErrorHandler.Log("Reward : " + (++i) + "/" + rewards.Count, ELogTag.Rewards);
                yield return DisplayReward(reward);

                // Wait for the player to touch the screen before displaying the next reward
                yield return new WaitUntil(() => myDepth == m_Depth && (Input.touchCount > 0 || Input.GetMouseButtonDown(0)));
                yield return null;                      // Ensure the coroutine yields at least once to avoid blocking the main thread
            }

            // list of rewards done beeing displayed, reduce level of depth
            m_Depth--;
        }

        IEnumerator DisplayReward(SReward reward)
        {
            ErrorHandler.Log("DisplayReward : " + reward.RewardName, ELogTag.Rewards);

            if (reward.RewardType == typeof(EChest) && Enum.TryParse(reward.RewardName, out EChest chestType))
            {
                yield return DisplayChestReward(chestType, reward.Qty);
            } 
            else if (reward.RewardType == typeof(ECurrency) && Enum.TryParse(reward.RewardName, out ECurrency currency))
            {
                yield return DisplayCurrencyReward(currency, reward.Qty);
            } 
            else if (ProfileCloudData.TryGetType(reward.RewardType, out EAchievementReward arType, false))
            {
                yield return DisplayAchievementReward(arType, reward.RewardName);
            }
            else
            {
                var collectable = CollectablesManagementData.Cast(reward.RewardName, reward.RewardType);
                if (collectable == null)
                {
                    ErrorHandler.Error("Unable to display reward " + reward.RewardName + " with type " + reward.RewardType);
                    yield break;
                }

                yield return DisplayCollectableReward(CollectablesManagementData.Cast(reward.RewardName, reward.RewardType), reward.Qty);
            }
        }

        #endregion


        #region Chests

        IEnumerator DisplayChestReward(EChest chestType, int qty)
        {
            ErrorHandler.Log("DisplayChestReward() : ", ELogTag.Rewards);
            ErrorHandler.Log("      + chestType : " + chestType, ELogTag.Rewards);
            ErrorHandler.Log("      + qty : " + qty, ELogTag.Rewards);

            if (qty > 1)
                ErrorHandler.Warning("Multiple Chests not handled yet");

            // displaying a list of rewards add a new depth in the coroutine management
            m_Depth++;

            // deactivate rewards display container
            m_RewardDisplayContainer.SetActive(false);
            // activate chest container
            m_ChestContainer.SetActive(true);

            // clean chest container
            UIHelper.CleanContent(m_ChestContainer);

            // set rewards of the chest as current chest rewards
            m_CurrentChestRewardData = ItemLoader.GetChestRewardData(chestType);

            // instantiate chest prefab
            m_ChestUI = m_CurrentChestRewardData.Instantiate(m_ChestContainer);
            m_ChestUI.ActivateIdle(true, true);

            // wait until touch to display reward
            yield return new WaitUntil(() => Input.touchCount > 0 || Input.GetMouseButtonDown(0));

            yield return OpenChest();

            yield return DisplayRewards(m_CurrentChestRewardData.GenerateRewards());
        }

        IEnumerator OpenChest()
        {
            yield return m_ChestUI.PlayOpenAnimation();
        }

        #endregion


        #region Single 

        IEnumerator DisplayCurrencyReward(ECurrency currency, int qty)
        {
            ErrorHandler.Log("DisplayCurrencyReward() : ", ELogTag.Rewards);
            ErrorHandler.Log("      + currency : " + currency, ELogTag.Rewards);

            // play sound effect
            SoundFXManager.PlayOnce(SoundFXManager.GoldsCollectedSoundFX);

            // activate rewards display container
            m_RewardDisplayContainer.SetActive(true);
            m_RewardInfosSection.SetActive(true);
            // deactivate chest container
            m_ChestContainer.SetActive(false);

            string title = currency.ToString();
            int currentlyOwnValue = InventoryManager.GetCurrency(currency);

            // init default template and clean previous content
            UIHelper.CleanContent(m_RewardIconSection);

            TemplateCurrencyItem template = Instantiate(AssetLoader.LoadTemplateItem("CurrencyItem"), m_RewardIconSection.transform).GetComponent<TemplateCurrencyItem>();
            template.Initialize(currency, qty);

            // remove button and collection fillbar from item
            template.AsIconOnly();

            m_CollectionQty.text = "+ " + qty.ToString();
            m_RewardTitle.text = title;

            // -- setup collection fill bar
            m_CollectionFillBar.Initialize(currentlyOwnValue, currentlyOwnValue + qty);
            yield return m_CollectionFillBar.CollectionAnimationCoroutine(qty);

            // add reward to collection of rewards
            InventoryManager.UpdateCurrency(currency, qty, m_Context);
        }

        IEnumerator DisplayCollectableReward(Enum collectable, int qty)
        {
            SoundFXManager.PlayOnce(SoundFXManager.RewardCollectedSoundFX);

            // activate rewards display container
            m_RewardDisplayContainer.SetActive(true);
            // deactivate chest containers
            m_ChestContainer.SetActive(false);

            // clean content before next display
            UIHelper.CleanContent(m_RewardIconSection);

            // if is character but has already been unlocked
            if (collectable.GetType() == typeof(ECharacter) && InventoryCloudData.Instance.GetCollectable(collectable).Level > 0)
            {
                qty = 250;
            }

            // setup ui of the new collectable
            SetUpTemplateItem(collectable, qty);
            SetUpRewardInfos(collectable, qty);

            // skip one frame to be sure that the layout components are adjusted properly
            yield return null;

            // -- play collectable animation
            yield return PlayRewardAnimation();
            // -- play collection fill bar animation
            yield return m_CollectionFillBar.CollectionAnimationCoroutine(qty);
            
            // add reward to collection of rewards
            InventoryManager.AddCollectable(collectable, qty);
        }

        IEnumerator DisplayAchievementReward(EAchievementReward arType, string value)
        {
            ErrorHandler.Log("DisplayAchievementReward : ", ELogTag.Rewards);
            ErrorHandler.Log("      + EAchievementReward : " + arType, ELogTag.Rewards);
            ErrorHandler.Log("      + value : " + value, ELogTag.Rewards);

            // play sound effect
            SoundFXManager.PlayOnce(SoundFXManager.AchievementRewardCollectedSoundFX);

            // activate rewards display container
            m_RewardDisplayContainer.SetActive(true);
            m_RewardInfosSection.SetActive(false);
            // deactivate chest containers
            m_ChestContainer.SetActive(false);

            // clean content before next display
            UIHelper.CleanContent(m_RewardIconSection);

            // setup ui of the new template
            SetUpAchievementRewardTemplate(arType, value);
            if (m_CurrentTemplateItem == null)
                yield break;

            yield return PlayAchievementRewardAnimation();

            AnimationHandler.AddRaycast(m_RewardIconSection, size: 2f, color: new Color(1f, 1f, 1f, 0.3f));

            // add reward to collection of rewards
            ProfileCloudData.AddAchievementReward(arType, value);

            // wait for click to display next
            yield return new WaitUntil(() => Input.touchCount > 0 || Input.GetMouseButtonDown(0));
        }

        void SetUpTemplateItem(Enum collectable, int qty)
        {
            m_CurrentTemplateItem = Instantiate(AssetLoader.LoadTemplateItem(collectable), m_RewardIconSection.transform);
            var template = m_CurrentTemplateItem.GetComponent<TemplateCollectableItemUI>();
            template.Initialize(collectable, true);
            template.SetMysteryIcon(true);
            template.ForceState(EButtonState.Normal);
        }

        void SetUpAchievementRewardTemplate(EAchievementReward ar, string value)
        {
            var template = Instantiate(AssetLoader.LoadAchievementRewardTemplate(ar), m_RewardIconSection.transform);
            if (template == null)
                return;

            m_CurrentTemplateItem = template.gameObject;
            template.Initialize(value, ar);
        }

        void SetUpRewardInfos(Enum collectable, int qty)
        {
            // get data from cloud manager
            SCollectableCloudData cloudData = InventoryCloudData.Instance.GetCollectable(collectable);

            m_RewardTitle.text = TextLocalizer.SplitCamelCase(collectable.ToString());
            m_CollectionQty.text = "+ " + qty.ToString();

            // -- setup collection fill bar
            m_CollectionFillBar.Initialize(cloudData.GetQty(), CollectablesManagementData.GetLevelData(collectable, cloudData.Level).RequiredQty);
        }

        #endregion


        #region Reward Animation

        /// <summary>
        /// Play animation of a new reward
        /// </summary>
        /// <returns></returns>
        IEnumerator PlayRewardAnimation()
        {
            // deactivate infos content && remove layout of TemplateIcon
            DisplayRewardInfosContent(false);
            var layoutElement = m_RewardIconSection.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            // animation of removing the mystery icon (if any)
            yield return RemoveMysteryIcon();

            // move on the side
            var move = m_RewardIconSection.AddComponent<MoveAnimation>();
            move.Initialize(duration: 0.2f, endPos: new Vector3(transform.position.x - 2, transform.position.y, transform.position.z));

            // wait until animation done playing or player skips it
            yield return new WaitUntil(() => move.IsOver);

            // re-activate infos content && set back layout of TemplateIcon
            DisplayRewardInfosContent(true);
            layoutElement.ignoreLayout = false;

            // fade-in animation on the Informations content
            var fadeIn = m_RewardInfosSection.AddComponent<Fade>();
            fadeIn.Initialize(duration: 0.2f, startOpacity: 0f, startScale: 0.8f);

            yield return new WaitUntil(() => fadeIn.IsOver);
        }

        IEnumerator RemoveMysteryIcon()
        {
            if (m_CurrentTemplateItem == null || m_CurrentTemplateItem.IsDestroyed())
            {
                ErrorHandler.Warning("Calling RemoveMysteryIcon() on item that no longer exists");
                yield break;
            }

            var componentUI = m_CurrentTemplateItem.GetComponent<TemplateCollectableItemUI>();
            if (componentUI == null)
                yield break;

            var rotation = m_CurrentTemplateItem.AddComponent<RotateAnimation>();
            rotation.Initialize(duration: 0.7f, rotation: new Vector3(0, 720, 0));

            yield return new WaitUntil(() => rotation.IsOver);

            if (componentUI.CollectableCloudData.Level == 0)
                yield return UnlockAnimation();
            
            componentUI.SetMysteryIcon(false);

            var raretyColor = CollectablesManagementData.GetRaretyData(componentUI.CollectableCloudData.GetCollectable()).Color;
            raretyColor.a = 0.6f;
            AnimationHandler.AddRaycast(m_RewardIconSection, color: raretyColor);

            yield return new WaitForSeconds(0.45f);
        }

        IEnumerator UnlockAnimation()
        {
            if (m_CurrentTemplateItem == null || m_CurrentTemplateItem.IsDestroyed())
            {
                ErrorHandler.Warning("Calling UnlockAnimation() on item that no longer exists");
                yield break;
            }

        }

        #endregion


        #region Achievement Rewards Animation

        IEnumerator PlayAchievementRewardAnimation()
        {
            var fadeIn = m_CurrentTemplateItem.AddComponent<Fade>();
            fadeIn.Initialize(duration: 0.35f, startScale: 0.8f, endScale:2f);

            yield return new WaitUntil(() => fadeIn.IsOver);
        }

        #endregion


        #region GUI Manipulators

        void DisplayRewardInfosContent(bool b)
        {
            foreach (Transform child in m_RewardInfosSection.transform)
            {
                child.gameObject.SetActive(b);
            }
        }

        #endregion
    }
}