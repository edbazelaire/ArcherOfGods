using System;
using System.Collections;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu.MainMenu.MainTab.Chests
{
    public class ChestUI : MonoBehaviour
    {
        #region Members

        const string IDLE_ANIMATION     = "ChestIdle";
        const string JUMP_ANIMATION     = "ChestJump";
        const string OPEN_ANIMATION     = "ChestOpen";

        const string c_ChestPreview     = "ChestPreview";
        const string c_AuraEffects      = "AuraEffects";
        const string c_OpenEffects      = "OpenEffects";

        GameObject  m_ChestPreview;
        Sprite      m_Icon;
        GameObject  m_AuraEffects;
        GameObject  m_OpeningEffects;
        Animator    m_Animator;

        public Sprite Icon => m_Icon;

        // EVENTS
        public Action OpenAnimationEndedEvent;

        #endregion


        #region Init & End

        public void Initialize()
        {
            m_ChestPreview      = Finder.Find(gameObject, c_ChestPreview);
            m_Icon              = Finder.FindComponent<SpriteRenderer>(m_ChestPreview).sprite;
            m_Animator          = Finder.FindComponent<Animator>(m_ChestPreview);
            m_AuraEffects       = Finder.Find(gameObject, c_AuraEffects);
            m_OpeningEffects    = Finder.Find(gameObject, c_OpenEffects);

            m_AuraEffects.SetActive(false);
            m_OpeningEffects.SetActive(false);

            OpenAnimationEndedEvent += OnOpenAnimationEnded;
        }

        private void OnDestroy()
        {
            OpenAnimationEndedEvent -= OnOpenAnimationEnded;
        }

        #endregion


        #region Animation & Particles

        public void ActivateIdle(bool withAura = false)
        {
            m_Animator.Play(IDLE_ANIMATION);
            ActivateAura(withAura);
        }

        public void ActivateOpen(bool withOpenParticles = true)
        {
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
            OpenAnimationEndedEvent?.Invoke();
        }

        #endregion


        #region Listeners

        void OnOpenAnimationEnded()
        {
            ActivateOpenParticles(false);
        }

        #endregion
    }
}