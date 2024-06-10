using Data;
using Enums;
using Game.Loaders;
using Game.Spells;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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
        /// <summary> list of visual effects proc by state effects</summary>
        List<GameObject>        m_StateEffectGraphics;
        /// <summary> list of colors of the state effect </summary>
        List<Color>             m_Colors;

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
            m_StateEffectGraphics = new();
            m_Colors = new();

            m_Controller.SpellHandler.IsCasting.OnValueChanged              += OnIsCastingValueChanged;
            m_Controller.StateHandler.StateEffectList.OnListChanged         += OnStateEffectListChanged;
            m_Controller.StateHandler.SpeedBonus.OnValueChanged             += OnSpeedBonusValueChanged;
            m_Controller.StateHandler.AnimationState.OnValueChanged         += OnStateAnimationValueChanged;
            m_Controller.CounterHandler.HasCounter.OnValueChanged           += OnHasCounterValueChanged;

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


        #region Public Manipulators

        /// <summary>
        /// change the color of this character on each clients
        /// </summary>
        /// <param name="color"></param>
        public void SpawnSpellEffectGraphics(GameObject visualEffect, string effectName)
        {
            if (visualEffect == null)
                return;

            ErrorHandler.Log("SpawnSpellEffectGraphics : " + effectName, ELogTag.Animation);

            var myVisualEffect = Instantiate(visualEffect, m_Controller.transform);
            myVisualEffect.name = effectName;
            m_StateEffectGraphics.Add(myVisualEffect);
        }

        /// <summary>
        /// change the color of this character on each clients
        /// </summary>
        /// <param name="color"></param>
        public void RemoveSpellEffectGraphics(string effectName)
        {
            int index = -1;
            for (int i = 0; i < m_StateEffectGraphics.Count; i++)
            {
                if (m_StateEffectGraphics[i].name == effectName)
                {
                    index = i;
                    ErrorHandler.Log("Removing spell effect : " + effectName, ELogTag.Animation);
                    break;
                }
            }    

            if (index < 0)
            {
                ErrorHandler.Error("Unable to find visual effect " + effectName + " in list of visual effects");
                return;
            }

            Destroy(m_StateEffectGraphics[index]);
            m_StateEffectGraphics.RemoveAt(index);
        }

        /// <summary>
        /// change the color of this character on each clients
        /// </summary>
        /// <param name="color"></param>
        [ClientRpc]
        public void AddColorClientRPC(Color color)
        {
            ErrorHandler.Log("Player (" + m_Controller.PlayerId + ") AddColorClientRPC : " + color, ELogTag.Animation);
            AddColor(color);
        }

        /// <summary>
        /// change the color of this character on each clients
        /// </summary>
        /// <param name="color"></param>
        [ClientRpc]
        public void RemoveColorClientRPC(Color color)
        {
            ErrorHandler.Log("Player (" + m_Controller.PlayerId + ") RemoveColorClientRPC : " + color, ELogTag.Animation);
            RemoveColor(color);
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
            ErrorHandler.Log("Setting Animation MovementSpeed factor : " + m_Controller.Movement.Speed, ELogTag.Animation);
            m_Animator.SetFloat("MovementSpeed", m_Controller.Movement.Speed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="win"></param>
        public void GameOverAnimation(bool win)
        {
            ErrorHandler.Log("Player (" + m_Controller.PlayerId + ") GameOverAnimation", ELogTag.Animation);
            m_Animator.SetTrigger(win ? EAnimation.Win.ToString() : EAnimation.Loss.ToString());
        }


        #endregion


        #region Private Manipulators

        public void Cast()
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

        void AddColor(Color color)
        {
            m_Colors.Add(color);
            SetColor(color);
        }

        void RemoveColor(Color color)
        {
            m_Colors.Remove(color);
            color = m_Colors.Count > 0 ? m_Colors.Last() : Color.white;
            SetColor(color);
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

        public void CastAnimation(EAnimation animation, float animationTimer)
        {
            if (animation == EAnimation.None)
                return;

            m_Animator.SetTrigger(animation.ToString());

            // update speed of the animation
            m_Animator.SetFloat("CastSpeed", m_Controller.SpellHandler.CurrentCastSpeedFactor / animationTimer);
        }

        void CastAnimation()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_Controller.SpellHandler.SelectedSpell);

            ErrorHandler.Log(spellData.Name + " animation : " + spellData.Animation, ELogTag.Animation);

            CastAnimation(spellData.Animation, spellData.AnimationTimer);
        }

        public void CancelCastAnimation()
        {
            ErrorHandler.Log("CancelCastAnimation : ", ELogTag.Animation);

            m_Animator.SetTrigger(EAnimation.CancelCast.ToString());
            m_Animator.SetFloat("CastSpeed", 1f);

            // Reset the trigger to avoid it staying "active"
            CoroutineManager.DelayMethod(() => m_Animator.ResetTrigger(EAnimation.CancelCast.ToString()));
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


        #region Helpers

        bool IsCurrentAnimation(string animation)
        {
            return m_Animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
        }

        #endregion


        #region Events Listeners

        void OnIsCastingValueChanged(bool oldValue, bool newValue)
        {
            if (oldValue == newValue)
                return;

            // start casting
            if (newValue)
                Cast();

            // end cast
            else
                CancelCast();
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
        /// Change MovementSpeed parameter in the Animator when the speed value changes
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnStateAnimationValueChanged(EAnimation oldValue, EAnimation newValue)
        {
            ErrorHandler.Log("STATE ANIMATION CHANGED : animation = " + newValue.ToString().ToUpper(), ELogTag.Animation);

            if (newValue == EAnimation.None)
                m_Animator.SetTrigger("StopStateAnimation");

            m_Animator.SetTrigger(newValue.ToString());
        }

        /// <summary>
        /// Change MovementSpeed parameter in the Animator when the speed value changes
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnHasCounterValueChanged(bool oldValue, bool newValue)
        {
            ErrorHandler.Log("STATE ANIMATION CHANGED : Counter = " + newValue.ToString().ToUpper(), ELogTag.Animation);

            m_Animator.SetBool("HasCounter", newValue);
        }

        /// <summary>
        /// Change counter of the character on proc
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnStateEffectListChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
        {
            ErrorHandler.Log(changeEvent.Type + " " + changeEvent.Value, ELogTag.Animation);

            if (changeEvent.Type != NetworkListEvent<FixedString64Bytes>.EventType.RemoveAt && changeEvent.Type != NetworkListEvent<FixedString64Bytes>.EventType.Remove)
                OnAddStateEffect(changeEvent.Value.ToString());
            else
                OnRemoveStateEffect(changeEvent.Value.ToString());

            // ---------------------------------------------------------------------------------------
            // SPECIAL EFFECTS
            if (changeEvent.Value == EStateEffect.Invisible.ToString())
            {
                float opacity = 1f;

                if (changeEvent.Type != NetworkListEvent<FixedString64Bytes>.EventType.RemoveAt)
                    opacity = IsOwner ? 0.5f : 0f;

                SetColor(new Color(1f, 1f, 1f, opacity));
                return;
            }

            if (changeEvent.Value == EStateEffect.Jump.ToString())
            {
                if (changeEvent.Type == NetworkListEvent<FixedString64Bytes>.EventType.Add)
                {
                    m_Controller.Collider.enabled = false;
                    m_Animator.SetTrigger(EAnimation.Jump.ToString());
                }
                else
                {
                    m_Controller.Collider.enabled = true;
                    m_Animator.SetTrigger(EAnimation.CancelCast.ToString());
                }
                return;
            }
        }

        void OnAddStateEffect(string stateEffectName)
        {
            StateEffect data = SpellLoader.GetStateEffect(stateEffectName);

            if (data.VisualEffect != null)
                SpawnSpellEffectGraphics(data.VisualEffect, stateEffectName);

            if (data.ColorSwitch != Color.white)
                AddColor(data.ColorSwitch);

            if (data.Animation != EAnimation.None)
                m_Animator.SetTrigger(data.Animation.ToString());
        }

        void OnRemoveStateEffect(string stateEffectName)
        {
            StateEffect data = SpellLoader.GetStateEffect(stateEffectName);
         
            if (data.VisualEffect != null)
                RemoveSpellEffectGraphics(stateEffectName);

            if (data.ColorSwitch != Color.white)
                RemoveColor(data.ColorSwitch);

            if (data.Animation != EAnimation.None)
                m_Animator.SetTrigger(EAnimation.CancelStateEffect.ToString());
        }
               
        #endregion
    }
}