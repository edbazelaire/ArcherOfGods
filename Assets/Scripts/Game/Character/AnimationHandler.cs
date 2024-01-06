using Data;
using Enums;
using Game.Managers;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

namespace Game.Character
{
    public class AnimationHandler : NetworkBehaviour
    {
        #region Members

        bool m_Initialized = false;

        /// <summary> controller of this AnimationHandler </summary>
        Controller m_Controller;

        /// <summary> sprite renderer of the Character</summary>
        SpriteRenderer m_SpriteRenderer;

        /// <summary> animator of the Character </summary>
        Animator m_Animator;

        /// <summary> particles displayed with the animation </summary>
        List<GameObject> m_AnimationPrefabs;

        #endregion


        #region Init & End

        public void Initialize(Animator animator)
        {
            m_Controller = GetComponent<Controller>();
            m_SpriteRenderer = Finder.FindComponent<SpriteRenderer>(m_Controller.CharacterPreview);
            m_Animator = animator;
            m_AnimationPrefabs = new List<GameObject>();

            m_Controller.SpellHandler.AnimationTimer.OnValueChanged += OnAnimationTimerChanged;
            m_Controller.CounterHandler.CounterProc.OnValueChanged += OnCounterProcChanged;
            m_Controller.StateHandler.StateEffectList.OnListChanged += OnStateEffectListChanged;

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

        void CastAnimation()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_Controller.SpellHandler.SelectedSpell);

            if (spellData.Animation == EAnimation.Count)
                return;

            m_Animator.SetTrigger(spellData.Animation.ToString());

            // not working anymore ????
            //m_Animator.SetFloat("CastSpeed", 1f / speed);
        }

        void CancelCastAnimation()
        {
            m_Animator.SetTrigger(EAnimation.CancelCast.ToString());
            m_Animator.SetFloat("CastSpeed", 1f);
        }

        #endregion


        #region Animation Spawns

        void StartAnimationParticles()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_Controller.SpellHandler.SelectedSpell);

            if (spellData.AnimationPrefabs.Count == 0)
                return;

            foreach (var prefab in spellData.AnimationPrefabs)
                m_AnimationPrefabs.Add(GameObject.Instantiate(prefab, m_Controller.SpellHandler.SpellSpawn));
        }

        /// <summary>
        /// Destroy animation particles if any
        /// </summary>
        void EndAnimationParticles()
        {
            if (m_AnimationPrefabs.Count == 0)
                return;

            foreach (var prefab in m_AnimationPrefabs)
                Destroy(prefab);

            m_AnimationPrefabs = new List<GameObject>();
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

        /// <summary>
        /// Change counter of the character on proc
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnCounterProcChanged(int oldValue, int newValue)
        {
            m_SpriteRenderer.color = newValue != (int)ESpell.Count ? Color.black : Color.white;
        }

        /// <summary>
        /// Change counter of the character on proc
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnStateEffectListChanged(NetworkListEvent<int> changeEvent)
        {
            Debug.Log($"OnStateEffectListChanged : {changeEvent.Type} {changeEvent.Value}");
            Debug.Log($"    + LocalClient : {NetworkManager.Singleton.LocalClient.ClientId}");
            Debug.Log($"    + Owner : {OwnerClientId}");

            switch ((EStateEffect)changeEvent.Value)
            {
                case EStateEffect.Invisible:
                    var baseColor = m_SpriteRenderer.color;

                    // if the character is visible, set the alpha to 1f
                    if (changeEvent.Type == NetworkListEvent<int>.EventType.RemoveAt) 
                        baseColor.a = 1f;

                    // invisible : set the alpha to 0.5f for owner and 0f for others
                    else
                        baseColor.a  = IsOwner ? 0.5f : 0f;

                    m_SpriteRenderer.color = baseColor;
                    break;

                default:
                    break;
            }
        }
               
        #endregion
    }
}