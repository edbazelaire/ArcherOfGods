using Enums;
using Save;
using Tools;
using UnityEngine.UI;

namespace Menu.Common.Buttons
{
    public class TemplateCharacterItemUI : TemplateItemButton
    {
        #region Members

        SCharacterCloudData m_CharacterCloudData;

        ECharacter m_Character => m_CharacterCloudData.Character;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_Border = Finder.FindComponent<Image>(gameObject, "IconContainer");

            CharacterBuildsCloudData.SelectedCharacterChangedEvent  += OnSelectedCharacterChanged;
            InventoryCloudData.CharacterDataChangedEvent            += OnCharacterDataChanged;
        }

        public void Initialize(ECharacter character)
        {
            base.Initialize();

            m_CharacterCloudData = InventoryCloudData.Instance.GetCharacter(character);
            m_Icon.sprite = AssetLoader.LoadCharacterIcon(character);

            // deactivate lock state by default (chars are all unlocked)
            m_LockState.SetActive(false);

            // setup ui depending on context
            RefreshUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CharacterBuildsCloudData.SelectedCharacterChangedEvent  -= OnSelectedCharacterChanged;
            InventoryCloudData.CharacterDataChangedEvent            -= OnCharacterDataChanged;
        }

        #endregion


        #region GUI Manipulators

        void RefreshUI()
        {
            m_LevelValue.text = string.Format(LEVEL_FORMAT, m_CharacterCloudData.Level);
            RefreshSelection();
        }

        void RefreshSelection()
        {
            m_OnSelected.SetActive(m_Character == CharacterBuildsCloudData.SelectedCharacter);
        }

        #endregion


        #region Listeners

        /// <summary>
        /// Action happening when the button is clicked on - depending on button context
        /// </summary>
        protected override void OnClick()
        {
            CharacterBuildsCloudData.SetSelectedCharacter(m_Character);
        }

        protected void OnSelectedCharacterChanged()
        {
            RefreshSelection();
        }

        protected void OnCharacterDataChanged(SCharacterCloudData data)
        {
            if (data.Character != m_CharacterCloudData.Character)
                return;

            m_CharacterCloudData = data;
            RefreshUI();
        }

        #endregion
    }
}