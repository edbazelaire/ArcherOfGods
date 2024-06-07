using Assets.Scripts.Managers.Sound;
using Data;
using Enums;
using Game.Loaders;
using Save;
using System;
using System.Collections;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu.MainMenu.MainTab.Chests
{
    public class ChestUI : MObject
    {
        #region Members

        const string IDLE_ANIMATION = "ChestIdle";
        const string JUMP_ANIMATION = "ChestJump";
        const string OPEN_ANIMATION = "ChestOpen";

        const string c_ChestPreview = "ChestPreview";
        const string c_AuraEffects = "AuraEffects";
        const string c_OpenEffects = "OpenEffects";

        ChestRewardData m_ChestData;

        GameObject m_ChestPreview;
        Sprite m_Icon;
        GameObject m_AuraEffects;
        GameObject m_OpeningEffects;
        Animator m_Animator;

        AudioSource m_AudioSource;

        public Sprite Icon => m_Icon;

        private EChest m_ChestType 
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
        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_ChestData = ItemLoader.GetChestRewardData(m_ChestType);

            m_ChestPreview      = Finder.Find(gameObject, c_ChestPreview);
            m_Icon              = Finder.FindComponent<SpriteRenderer>(m_ChestPreview).sprite;
            m_Animator          = Finder.FindComponent<Animator>(m_ChestPreview);
            m_AuraEffects       = Finder.Find(gameObject, c_AuraEffects);
            m_OpeningEffects    = Finder.Find(gameObject, c_OpenEffects);
        }

        protected override void SetUpUI()
        {
            base.SetUpUI();

            m_AuraEffects.SetActive(false);
            m_OpeningEffects.SetActive(false);
        }
        
        #endregion


        #region Animation & Particles

        public void ActivateIdle(bool withAura = false, bool withSound = false)
        {
            m_Animator.Play(IDLE_ANIMATION);
            ActivateAura(withAura);

            if (withSound && m_ChestData.IdleSoundFX != null)
                m_AudioSource = SoundFXManager.PlaySoundFXClip(m_ChestData.IdleSoundFX);
        }

        public void ActivateOpen(bool withOpenParticles = true)
        {
            if (m_AudioSource.isActiveAndEnabled)
                Destroy(m_AudioSource);

            StartCoroutine(PlayOpenAnimation());
        }

        public void ActivateAura(bool activate = true)
        {
            m_AuraEffects.SetActive(activate);
        }

        public void ActivateOpenParticles(bool activate = true)
        {
            m_OpeningEffects.SetActive(activate);
        }

        public IEnumerator PlayOpenAnimation()
        {
            m_Animator.Play(OPEN_ANIMATION);
            ActivateOpenParticles(true);

            if (m_AudioSource != null)
                Destroy(m_AudioSource);

            SoundFXManager.PlayOnce(m_ChestData.OpenSoundFX != null ? m_ChestData.OpenSoundFX : SoundFXManager.DefaultChestOpenSoundFX);

            // wait for the animation to start
            while (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName(OPEN_ANIMATION))
            {
                yield return null;
            }

            // wait for the end of the animation
            while (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(OPEN_ANIMATION))
            {
                yield return null;
            }

            ActivateOpenParticles(false);
        }

        #endregion
    }
}