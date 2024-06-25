using Assets.Scripts.Menu.MainMenu.InventoryTab;
using Enums;
using Save;
using System.Linq;
using Tools;
using UnityEngine;

namespace Menu.MainMenu
{
    public class CurrentBuildDisplayUI : MonoBehaviour
    {
        #region Members

        const string FORMAT_SPELL_ITEM_CONTAINER = "SpellItemContainer ({0})";
        const int N_SPELL_ITEM_CONTAINERS = CharacterBuildsCloudData.N_SPELLS_IN_BUILDS;

        static CurrentBuildDisplayUI s_Instance;
        public static CurrentBuildDisplayUI Instance => s_Instance;

        SelectedSpellWindow m_SelecetedSpellWindow;
        SpellItemContainerUI[] m_SpellItemContainers;

        public static ESpell? CurrentSelectedCard = null;

        #endregion


        #region Init & End

        private void Awake()
        {
            m_SelecetedSpellWindow = Finder.FindComponent<SelectedSpellWindow>("SelectedSpellWindow");
            m_SpellItemContainers = new SpellItemContainerUI[N_SPELL_ITEM_CONTAINERS];
            for (int i = 0; i < N_SPELL_ITEM_CONTAINERS; i++)
            {
                var spellContainerUI = Finder.FindComponent<SpellItemContainerUI>(string.Format(FORMAT_SPELL_ITEM_CONTAINER, i));
                spellContainerUI.Initialize(i);

                m_SpellItemContainers[i] = spellContainerUI;
            }

            m_SelecetedSpellWindow.Initialize();
            s_Instance = this;
        }

        #endregion


        #region Spell Management

        public static void SetCurrentSelectedCard(ESpell? spell)
        {
            // if already have a spell selected, check if this spell should be displayed 
            if (spell.HasValue)
                Instance.m_SelecetedSpellWindow.Activate(spell.Value);
            else
                Instance.m_SelecetedSpellWindow.Deactivate();

            CurrentSelectedCard = spell;
        }

        /// <summary>
        /// Remove a spell from current build (does not save in database thaught, as we do not want to save uncompleted builds)
        /// </summary>
        /// <param name="spell"></param>
        public static void RemoveSpell(ESpell spell)
        {
            foreach (var spellItemContainer in Instance.m_SpellItemContainers)
            {
                if (spellItemContainer.Spell == spell)
                {
                    spellItemContainer.RemoveCurrentSpell();
                    return;
                }
            }

            ErrorHandler.Error("Unable to find spell " + spell + " in current build values");
        }

        /// <summary>
        /// Find fist empty slot to set the spell. Return true if any, otherwise false
        /// </summary>
        /// <returns></returns>
        public static bool UseFirstEmptySlot(ESpell spell)
        {
            foreach (var spellItemContainer in Instance.m_SpellItemContainers)
            {
                if (spellItemContainer.Spell == null)
                {
                    spellItemContainer.SetSpell(spell);
                    SetCurrentSelectedCard(null);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Replace a spell wi
        /// </summary>
        /// <param name="spell"></param>
        public static void ReplaceSpell(ESpell spell)
        {
            if (CurrentSelectedCard == null)
            {
                ErrorHandler.Error("Trying to replace spell but the CurrentlyUsedSpell is not set");
                return;
            }

            foreach (var spellItemContainer in Instance.m_SpellItemContainers)
            {
                if (spellItemContainer.Spell == spell)
                {
                    spellItemContainer.SetSpell(CurrentSelectedCard.Value);
                    SetCurrentSelectedCard(null);
                    return;
                }
            }

            ErrorHandler.Error("Unable to find spell " + spell + " in current build values");
        }

        #endregion

    }
}