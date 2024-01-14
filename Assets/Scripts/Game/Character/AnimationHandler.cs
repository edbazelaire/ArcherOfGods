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
        List<SpriteRenderer> m_SpriteRenderers;

        /// <summary> animator of the Character </summary>
        Animator m_Animator;

        /// <summary> particles displayed with the animation </summary>
        List<GameObject> m_AnimationPrefabs;

        #endregion


        #region Init & End

        public void Initialize(Animator animator)
        {
            if (animator == null)
            {
                ErrorHandler.Error("animator not found");
            }

            m_Controller = GetComponent<Controller>();
            m_SpriteRenderers = Finder.FindComponents<SpriteRenderer>(m_Controller.CharacterPreview);
            m_Animator = animator;
            m_AnimationPrefabs = new List<GameObject>();

            m_Controller.SpellHandler.AnimationTimer.OnValueChanged         += OnAnimationTimerChanged;
            m_Controller.CounterHandler.CounterActivated.OnValueChanged     += OnCounterActivatedValueChanged;
            m_Controller.StateHandler.StateEffectList.OnListChanged         += OnStateEffectListChanged;

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


        #region Client RPC

        [ClientRpc]
        public void ChangeColorClientRPC(Color color)
        {
            foreach (var spriteRenderer in m_SpriteRenderers)
                spriteRenderer.color = color;
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
            m_Animator.SetFloat("CastSpeed", 1f / spellData.Speed);
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
        void OnCounterActivatedValueChanged(bool oldValue, bool newValue)
        {
            if (newValue == true)
            {
                m_Animator.SetTrigger(EAnimation.Counter.ToString());
            } else
            {
                m_Animator.SetTrigger(EAnimation.CancelCast.ToString());
            }
        }

        /// <summary>
        /// Change counter of the character on proc
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnStateEffectListChanged(NetworkListEvent<int> changeEvent)
        {
            switch ((EStateEffect)changeEvent.Value)
            {
                case EStateEffect.Invisible:
                    float opacity = 1f;

                    if (changeEvent.Type != NetworkListEvent<int>.EventType.RemoveAt)
                        opacity = IsOwner ? 0.5f : 0f;

                    foreach (var spriteRenderer in m_SpriteRenderers)
                    {
                        var baseColor = spriteRenderer.color;
                        baseColor.a = opacity;
                        spriteRenderer.color = baseColor;
                    }
                        
                    break;

                case (EStateEffect.Jump):
                    if (changeEvent.Type == NetworkListEvent<int>.EventType.Add)
                    {
                        m_Controller.Collider.enabled = false;
                        m_Animator.SetTrigger(EAnimation.Jump.ToString());
                        
                    } else
                    {
                        m_Controller.Collider.enabled = true;
                        m_Animator.SetTrigger(EAnimation.CancelCast.ToString());
                    }
                    
                    break;

                default:
                    break;
            }
        }
               
        #endregion
    }
}