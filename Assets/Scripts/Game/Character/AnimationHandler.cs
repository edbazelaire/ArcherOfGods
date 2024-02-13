using Data;
using Enums;
using Game.Managers;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class AnimationHandler : NetworkBehaviour
    {
        #region Members

        bool m_Initialized = false;

        /// <summary> controller of this AnimationHandler </summary>
        Controller              m_Controller;

        /// <summary> sprite renderer of the Character</summary>
        List<SpriteRenderer>    m_SpriteRenderers;

        /// <summary> animator of the Character </summary>
        Animator                m_Animator;

        /// <summary> particles displayed with the animation </summary>
        List<GameObject>        m_AnimationPrefabs;

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
            m_Controller.StateHandler.SpeedBonus.OnValueChanged             += OnSpeedBonusValueChanged;

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

        #endregion


        #region Public Manipulators

        /// <summary>
        /// change the color of this character on each clients
        /// </summary>
        /// <param name="color"></param>
        [ClientRpc]
        public void ChangeColorClientRPC(Color color)
        {
            foreach (var spriteRenderer in m_SpriteRenderers)
                spriteRenderer.color = color;
        }

        /// <summary>
        /// Call clients to hide/display a character
        /// </summary>
        /// <param name="hidden"></param>
        [ClientRpc]
        public void HideCharacterClientRPC(bool hidden)
        {
            HideCharacter(hidden);
        }

        /// <summary>
        /// Hide / show the character colors
        /// </summary>
        /// <param name="hidden"></param>
        public void HideCharacter(bool hidden)
        {
            Color color = hidden ? new Color(0f, 0f, 0f, 0f) : Color.white;
            SetColor(color);
        }


        /// <summary>
        /// Update Animation MovementSpeed factor depending on current speed
        /// </summary>
        public void UpdateMovementSpeed()
        {
            Debug.Log("Setting Animation MovementSpeed factor : " + m_Controller.Movement.Speed);
            m_Animator.SetFloat("MovementSpeed", m_Controller.Movement.Speed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="win"></param>
        public void GameOverAnimation(bool win)
        {
            m_Animator.SetTrigger(win ? "Win" : "Lose");
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

        void SetColor(Color color)
        {
            foreach (var spriteRenderer in m_SpriteRenderers)
                spriteRenderer.color = color;
        }

        #endregion


        #region Animations Manipulators

        void MoveAnimation(bool isMoving)
        {
            m_Animator.SetBool("IsMoving", isMoving);
        }

        void CastAnimation()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_Controller.SpellHandler.SelectedSpell);

            if (spellData.Animation == EAnimation.Count)
                return;

            m_Animator.SetTrigger(spellData.Animation.ToString());

            // update speed of the animation
            m_Animator.SetFloat("CastSpeed", 1f / spellData.AnimationTimer);
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

            if (spellData.OnAnimation.Count == 0)
                return;

            foreach (SPrefabSpawn onAnimationPrefab in spellData.OnAnimation)
                m_AnimationPrefabs.Add(onAnimationPrefab.Spawn(m_Controller, spellData.AnimationTimer));
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
                Cast();

            // to 0 : end casting
            else if (oldValue > 0 && newValue <= 0)
                CancelCast();
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
        /// Change MovementSpeed parameter in the Animator when the speed value changes
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnSpeedBonusValueChanged(float oldValue, float newValue)
        {
            UpdateMovementSpeed();
        }

        /// <summary>
        /// Change counter of the character on proc
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnStateEffectListChanged(NetworkListEvent<int> changeEvent)
        {
            Debug.Log(changeEvent.Type + " " + (EStateEffect)changeEvent.Value);

            switch ((EStateEffect)changeEvent.Value)
            {
                case EStateEffect.Invisible:
                    float opacity = 1f;

                    if (changeEvent.Type != NetworkListEvent<int>.EventType.RemoveAt)
                        opacity = IsOwner ? 0.5f : 0f;

                    SetColor(new Color(1f, 1f, 1f, opacity));
                    break;

                case (EStateEffect.IronSkin):
                    SetColor(changeEvent.Type == NetworkListEvent<int>.EventType.Add ? Color.grey : Color.white);
                    break;

                case (EStateEffect.Cursed):
                    SetColor(changeEvent.Type == NetworkListEvent<int>.EventType.Add ? Color.magenta : Color.white);
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