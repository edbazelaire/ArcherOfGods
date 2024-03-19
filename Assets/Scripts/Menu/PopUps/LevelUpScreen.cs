using Assets;
using Data;
using Data.GameManagement;
using Enums;
using Game.Managers;
using Inventory;
using Managers;
using MyBox;
using Save;
using System.Collections;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class LevelUpScreen : OverlayScreen
    {
        #region Members

        const string            LEVEL_FORMAT        = "Level {0}";
        const string            GOLDS_QTY_FORMAT    = "+ {0} Golds";

        // =====================================================================================
        // GameObjects & Components
        TMP_Text                m_Title;
        GameObject              m_CharacterPreviewContainer;
        RectTransform           m_CharacterPreviewRectTransform;
        GameObject              m_InfosSection;
        TMP_Text                m_CharacterName;
        TMP_Text                m_Level;
        TMP_Text                m_GoldsQty;
        GameObject              m_ChestsRewardDisplay;

        // =====================================================================================
        // Screen Behavior
        Coroutine               m_Coroutine;
        int                     m_RewardIndex;           

        // =====================================================================================
        // Data
        ECharacter              m_Character;
        CharacterData           m_CharacterData;
        SCharacterCloudData     m_CharacterCloudData;
        SCharacterLevelData     m_CharacterLevelData;

        #endregion


        #region Constructor

        public LevelUpScreen() : base(EPopUpState.LevelUpScreen) { }

        #endregion


        #region Init & End

        public void Initialize(ECharacter character)
        {
            m_Character             = character;
            m_CharacterData         = CharacterLoader.GetCharacterData(character);
            m_CharacterCloudData    = InventoryCloudData.Instance.GetCharacter(character);
            m_CharacterLevelData    = CharacterLoader.CharactersManagementData.GetCharacterLevelData(m_CharacterCloudData.Level - 1);   // get rewards of the previous level, since levelup has already happenned

            // reset 
            m_Coroutine             = null;
            m_RewardIndex           = 0;

            // add golds to inventory manager
            InventoryManager.AddGolds(m_CharacterLevelData.BonusGolds);

            base.Initialize();  
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            var titleContainer              = Finder.Find(gameObject, "TitleContainer");
            m_Title                         = Finder.FindComponent<TMP_Text>(titleContainer, "Title");
            m_CharacterPreviewContainer     = Finder.Find(gameObject, "CharacterPreviewContainer");
            m_CharacterPreviewRectTransform = Finder.FindComponent<RectTransform>(m_CharacterPreviewContainer);
            m_InfosSection                  = Finder.Find(gameObject, "InfosSection");
            m_CharacterName                 = Finder.FindComponent<TMP_Text>(m_InfosSection, "CharacterName");
            m_Level                         = Finder.FindComponent<TMP_Text>(m_InfosSection, "Level");
            m_GoldsQty                      = Finder.FindComponent<TMP_Text>(m_InfosSection, "GoldsQty");
            m_ChestsRewardDisplay           = Finder.Find(m_InfosSection, "ChestsRewardDisplay");

            SetUpUI();
        }

        #endregion


        #region GUI Manipulators

        /// <summary>
        /// Setup all UI Elements based on the Character
        /// </summary>
        void SetUpUI()
        {
            DisplayCharacterPreview();
            DisplayCharacterInfos();
            DisplayLevelUpRewards();
        }

        /// <summary>
        /// Display prefab of the Character in the container and scale it
        /// </summary>
        void DisplayCharacterPreview()
        {
            // clean current preview
            UIHelper.CleanContent(m_CharacterPreviewContainer);

            // get selected character preview
            var characterPreview = m_CharacterData.InstantiateCharacterPreview(m_CharacterPreviewContainer);

            // display character preview
            var baseScale = characterPreview.transform.localScale * m_CharacterData.Size;
            float scaleFactor = 0.6f * m_CharacterPreviewRectTransform.rect.height / characterPreview.transform.localScale.y;
            characterPreview.transform.localScale = new Vector3(baseScale.x * scaleFactor, baseScale.y * scaleFactor, 1f);
        }

        /// <summary>
        /// Update character infos (name, level, ...)
        /// </summary>
        void DisplayCharacterInfos()
        {
            m_CharacterName.text = m_Character.ToString();
            m_Level.text = string.Format(LEVEL_FORMAT, m_CharacterCloudData.Level);
        }

        /// <summary>
        /// Display all rewards from leveling up
        /// </summary>
        void DisplayLevelUpRewards()
        {
            m_GoldsQty.text = string.Format(GOLDS_QTY_FORMAT, m_CharacterLevelData.BonusGolds);

            // clean the content of the chests reward display (if any)
            UIHelper.CleanContent(m_ChestsRewardDisplay);

            // for each chests, add new gameobject with image component
            foreach (EChestType chestType in m_CharacterLevelData.BonusChests)
            {
                var chest = Instantiate(AssetLoader.LoadTemplateItem("Icon"), m_ChestsRewardDisplay.transform);
                chest.GetComponent<Image>().sprite = AssetLoader.LoadChestIcon(chestType);
            }
        }

        #endregion


        #region Rewards Display

        IEnumerator ChestsDisplayCoroutine()
        {
            while (m_RewardIndex < m_CharacterLevelData.BonusChests.Count)
            {
                DisplayNextReward();

                // wait for ChestOpeningScreen to be displayed
                while (Finder.FindComponent<ChestOpeningScreen>(Main.Canvas.gameObject, throwError: false) == null)
                {
                    yield return null;
                }

                // wait for ChestOpeningScreen to not be diplayed anymore
                while (Finder.FindComponent<ChestOpeningScreen>(Main.Canvas.gameObject, throwError: false) != null)
                {
                    yield return null;
                }
            }

            OnExit();
        }

        void DisplayNextReward()
        {
            Main.SetPopUp(EPopUpState.ChestOpeningScreen, m_CharacterLevelData.BonusChests[m_RewardIndex]);

            m_RewardIndex++;
        }

        #endregion


        #region Listeners

        protected override void OnTouch(GameObject gameObject)
        {
            if (m_Coroutine != null)
                return;

            base.OnTouch(gameObject);

            m_Coroutine = StartCoroutine(ChestsDisplayCoroutine());
        }

        #endregion
    }
}