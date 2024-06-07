using Assets;
using Assets.Scripts.Managers.Sound;
using Data.GameManagement;
using Enums;
using Menu.Common.Displayers;
using Menu.Common.Notifications;
using Menu.MainMenu;
using Menu.MainMenu.MainTab;
using Save;
using Tools;
using Tools.Animations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    enum EStageRewardState
    {
        Locked,
        Unlocked,
        Collected
    }

    public class StageDisplayUI : MObject
    {
        #region Members
       
        SArenaLevelData     m_ArenaLevelData;
        int                 m_ArenaLevel;
        EArenaType          m_ArenaType;

        EStageRewardState   m_State;
        NotificationDisplay m_NotificationDisplay;

        StageSectionUI      m_StageSectionUI;
        GameObject          m_SpellsContainer;
        RewardsDisplayer    m_RewardsDisplayer;
        GameObject          m_OverlayScreen;
        Button              m_CollectButton;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_StageSectionUI                = Finder.FindComponent<StageSectionUI>(gameObject);
            m_SpellsContainer               = Finder.Find(gameObject, "SpellsContainer");
            m_RewardsDisplayer              = Finder.FindComponent<RewardsDisplayer>(gameObject);
            m_NotificationDisplay           = Finder.FindComponent<NotificationDisplay>(m_RewardsDisplayer.gameObject);
            m_OverlayScreen                 = Finder.Find(m_RewardsDisplayer.gameObject, "OverlayScreen");
            m_CollectButton                 = Finder.FindComponent<Button>(gameObject, "CollectButton");

        }

        public void Initialize(ArenaData arenaData, int arenaLevel, EArenaType arenaType)
        {
            m_ArenaLevelData = arenaData.GetArenaLevelData(arenaLevel);
            m_ArenaLevel = arenaLevel;
            m_ArenaType = arenaType;

            base.Initialize();

            m_StageSectionUI.Initialize(arenaData, arenaLevel);
            m_NotificationDisplay.Initialize(Finder.FindComponent<Image>(m_RewardsDisplayer.gameObject), Vector2.one * 2);

            RefreshState();
            SetUpSpells();
            SetUpRewards();
        }

        #endregion


        #region GUI Manipulators

        void SetUpSpells()
        {
            UIHelper.CleanContent(m_SpellsContainer);
            foreach (ESpell spell in m_ArenaLevelData.Spells)
            {
                TemplateSpellItemUI spellItemUI = Instantiate(AssetLoader.LoadTemplateItem(spell), m_SpellsContainer.transform).GetComponent<TemplateSpellItemUI>();
                spellItemUI.Initialize(spell, asIconOnly: true);

                // TODO : later
                int spellLevel = m_ArenaLevelData.StageData[0].CharacterLevel;
                spellItemUI.SetBottomOverlay("Level " + spellLevel);

                // display informations of the spell on click
                spellItemUI.Button.interactable = true;
                spellItemUI.Button.onClick.RemoveAllListeners();
                spellItemUI.Button.onClick.AddListener(() => { Main.SetPopUp(EPopUpState.SpellInfoPopUp, spell, spellLevel, true); });
            }
        }

        void SetUpRewards()
        {
            m_RewardsDisplayer.Initialize(m_ArenaLevelData.rewardsData, m_ArenaLevelData.rewardsData.Count <= 4 ? 2 : 3);
        }

        #endregion


        #region State

        void RefreshState()
        {
            if (NotificationCloudData.HasRewardsForArenaTypeAtLevel(m_ArenaType, m_ArenaLevel))
            {
                SetState(EStageRewardState.Unlocked);
                return;
            }

            if (m_ArenaLevel < ProgressionCloudData.SoloArenas[m_ArenaType].CurrentLevel)
            {
                SetState(EStageRewardState.Collected);
                return;
            }

            SetState(EStageRewardState.Locked);
        }

        void SetState(EStageRewardState state)
        {
            m_State = state;

            switch (state)
            {
                case EStageRewardState.Locked:
                    m_NotificationDisplay.Deactivate();
                    m_CollectButton.gameObject.SetActive(false);
                    m_OverlayScreen.SetActive(false);
                    return;

                case EStageRewardState.Unlocked:
                    m_NotificationDisplay.Activate(); 
                    m_CollectButton.gameObject.SetActive(true);
                    var pulse = m_CollectButton.AddComponent<Pulse>();
                    pulse.Initialize("", -1, pauseDuration: 1.5f);

                    m_OverlayScreen.SetActive(false);
                    return;
                    
                case EStageRewardState.Collected:
                    m_NotificationDisplay.Deactivate();
                    m_CollectButton.gameObject.SetActive(false);
                    m_OverlayScreen.SetActive(true);
                    return;

                default: 
                    ErrorHandler.Error("Unknown state : " + state);
                    return;
            }
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();
          
            m_CollectButton.onClick.AddListener(OnCollectedButtonClicked);
        }

        protected override void UnRegisterListeners()
        {
            base.UnRegisterListeners();
         
            m_CollectButton.onClick.RemoveListener(OnCollectedButtonClicked);
        }

        void OnCollectedButtonClicked()
        {
            SoundFXManager.PlayOnce(SoundFXManager.ClickButtonSoundFX);

            if (! NotificationCloudData.CollectArenaReward(m_ArenaType, m_ArenaLevel))
                return;

            Main.DisplayRewards(m_ArenaLevelData.rewardsData, ERewardContext.ArenaReward);
            RefreshState();
        }

        #endregion
    }
}