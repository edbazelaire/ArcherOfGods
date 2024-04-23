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
        public void SpawnSpellEffectGraphics(GameObject visualEffect, string name)
        {
            if (visualEffect == null)
                return;

            var myVisualEffect = Instantiate(visualEffect, m_Controller.transform);
            myVisualEffect.name = name;
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
            AddColor(color);
        }

        /// <summary>
        /// change the color of this character on each clients
        /// </summary>
        /// <param name="color"></param>
        [ClientRpc]
        public void RemoveColorClientRPC(Color color)
        {
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
            Debug.Log("Setting Animation MovementSpeed factor : " + m_Controller.Movement.Speed);
            m_Animator.SetFloat("MovementSpeed", m_Controller.Movement.Speed);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="win"></param>
        public void GameOverAnimation(bool win)
        {
            m_Animator.SetTrigger(win ? EAnimation.Win.ToString() : EAnimation.Loss.ToString());
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

        void CastAnimation()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_Controller.SpellHandler.SelectedSpell);

            if (spellData.Animation == EAnimation.None)
                return;

            m_Animator.SetTrigger(spellData.Animation.ToString());

            // update speed of the animation
            m_Animator.SetFloat("CastSpeed", m_Controller.SpellHandler.CurrentCastSpeedFactor / spellData.AnimationTimer );
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
        /// Change counter of the character on proc
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        void OnStateEffectListChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
        {
            ErrorHandler.Log(changeEvent.Type + " " + changeEvent.Value, ELogTag.Spells);

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
        }
               
        #endregion
    }
}