using Data;
using Enums;
using Game.Managers;
using Tools;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

namespace Game.Character
{
    public class AnimationHandler : NetworkBehaviour
    {
        #region Members

        bool m_Initialized = false;

        /// <summary> controller of this AnimationHandler </summary>
        Controller m_Controller;
        
        /// <summary> animator of this AnimationHandler </summary>
        Animator m_Animator;

        /// <summary> particles displayed with the animation </summary>
        GameObject m_AnimationParticles;

        #endregion


        #region Init & End

        public void Initialize(Animator animator)
        {
            m_Controller = GetComponent<Controller>();
            m_Animator = animator;

            m_Controller.SpellHandler.AnimationTimer.OnValueChanged += OnAnimationTimerChanged;

            m_Initialized = true;
        }

        #endregion


        #region Inherited Manipulators

        public void Update()
        {
            if (!m_Initialized)
                return;

            MoveAnimation(m_Controller.Movement.IsMoving);
        }

        #endregion


        #region Private Manipulators

        void Cast()
        {
            // play animation of CAST
            CastAnimation();

            // create particles displayed during the animation
            StartAnimationParticles();
        }

        void CancelCast()
        {
            // stop animation of CAST
            CancelCastAnimation();

            // destroy particles displayed during the animation
            EndAnimationParticles();

        }

        #endregion


        #region Animations Manipulators

        void MoveAnimation(bool isMoving)
        {
            // todo later : handle animation cancels
            //var info = m_Animator.GetCurrentAnimatorStateInfo(0);
            //if (isMoving)
            //    CancelCast();

            m_Animator.SetBool("IsMoving", isMoving);
        }

        #endregion


        #region Animation Partiles

        void CastAnimation()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_Controller.SpellHandler.SelectedSpell);

            switch (spellData.Trajectory)
            {
                case ESpellTrajectory.Straight:
                    m_Animator.SetTrigger("CastShootStraight");
                    break;

                case ESpellTrajectory.Curve:
                    m_Animator.SetTrigger("CastShoot");
                    break;

                default:
                    Debug.LogError($"Trajectory {spellData.Trajectory} not implemented for spell {spellData.Name}");
                    break;
            }

            // not working anymore ????
            //m_Animator.SetFloat("CastSpeed", 1f / speed);
        }

        void CancelCastAnimation()
        {
            m_Animator.SetTrigger("CancelCast");
            m_Animator.SetFloat("CastSpeed", 1f);
        }

        void StartAnimationParticles()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_Controller.SpellHandler.SelectedSpell);

            if (spellData.AnimationParticles == null)
                return;

            m_AnimationParticles = GameObject.Instantiate(spellData.AnimationParticles, m_Controller.SpellHandler.SpellSpawn);
        }

        /// <summary>
        /// Destroy animation particles if any
        /// </summary>
        void EndAnimationParticles()
        {
            if (m_AnimationParticles == null)
                return;
            Destroy(m_AnimationParticles);
            m_AnimationParticles = null;
        }

        #endregion


        #region Events Listeners

        void OnAnimationTimerChanged(float oldValue, float newValue)
        {
            if (oldValue > 0 && newValue > 0)
                return;

            // from 0 : start casting
            if (oldValue <= 0 && newValue > 0)
            {
                Cast();
            }

            // to 0 : end casting
            else if (oldValue > 0 && newValue <= 0)
            {
                CancelCast();
            }
        }

        #endregion
    }
}