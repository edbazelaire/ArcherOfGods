﻿using Data;
using Enums;
using Game.Managers;
using Game.Spells;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Tools;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Character
{
    public class SpellHandler : NetworkBehaviour
    {
        #region Members

        const string                        c_SpellSpawn            = "SpellSpawn";
        const string                        c_TargettableArea       = "TargettableArea";

        public Action<ESpells>              SelectedSpellEvent;
        public Action                       CastEndedEvent;

        NetworkList<float>                  Cooldowns;
        NetworkList<int>                    m_SpellsNet;
        NetworkVariable<int>                m_SelectedSpellNet      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        NetworkVariable<float>              m_AnimationTimer        = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        NetworkVariable<Vector3>            m_TargetPos             = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        Controller m_Controller;
        Transform                           m_TargettableArea;
        Transform                           m_SpellSpawn;
        GameObject                          m_AnimationParticles;

        public NetworkVariable<int> SelectedSpellNet => m_SelectedSpellNet;
        public float AnimationTimer => m_AnimationTimer.Value;

        #endregion


        #region Inherited Manipulators

        private void Awake()
        {
            m_SpellsNet = new NetworkList<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
            Cooldowns   = new NetworkList<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        }

        public override void OnNetworkSpawn()
        {
            m_Controller = Finder.FindComponent<Controller>(gameObject);
            m_SpellSpawn = Finder.FindComponent<Transform>(gameObject, c_SpellSpawn);
            m_TargettableArea = Finder.FindComponent<Transform>(c_TargettableArea);
        }

        void Update()
        {
            if (! IsOwner)
                return; 

            CheckActionExectution();
            UpdateCooldownsServerRPC();
        }

        #endregion


        #region Initialization

        /// <summary>
        /// Initialize the spell handler
        /// </summary>
        /// <param name="spells"></param>
        public void Initialize(List<ESpells> spells)
        {
            if (!IsOwner)
                return;

            foreach (ESpells spell in spells)
            {
                m_SpellsNet.Add((int)spell);
                Cooldowns.Add(0);
            }
        }

        #endregion


        #region Server RPC

        [ServerRpc]
        /// <summary>
        /// Update cooldowns
        /// </summary>
        void UpdateCooldownsServerRPC()
        {
            foreach (ESpells spell in m_Spells)
            {
                SetCooldown(spell, GetCooldown(spell) - Time.deltaTime);
            }
        }

        [ServerRpc]
        void CastServerRPC()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);
            
            // setup cooldown and cast the spell
            SetCooldown(m_SelectedSpell, spellData.Cooldown);
            var spawnPosition = m_SpellSpawn.position + GetSpawnOffset(spellData.Trajectory);
            spellData.Cast(m_Controller.OwnerClientId, m_TargetPos.Value, spawnPosition, m_SpellSpawn.rotation);

            SelectSpell(m_Spells[0]);
        }

        [ServerRpc]
        void StartAnimationServerRPC()
        {
            // display animation on client side
            StartAnimationClientRPC();
        }

        [ServerRpc]
        void EndAnimationServerRPC()
        {
            // display animation on client side
            EndAnimationClientRPC();
        }

        #endregion


        #region Client RPC

        [ClientRpc]
        void StartAnimationClientRPC()
        {
            StartAnimation();
        }

        [ClientRpc]
        void EndAnimationClientRPC()
        {
            EndAnimation();
        }

        #endregion


        #region Animation

        /// <summary>
        /// 
        /// </summary>
        void StartAnimation()
        {
            StartAnimator();
            StartAnimationParticles();
        }

        /// <summary>
        /// Reset animator on end/cancel of animation
        /// </summary>
        void EndAnimation()
        {
            // cancel animation
            EndAnimator();

            // destroy animation particles
            EndAnimationParticles();

            // fire envent that the spell is over
            CastEndedEvent?.Invoke();
        }

        /// <summary>
        /// Trigger animation of the spell
        /// </summary>
        void StartAnimator()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);

            switch (spellData.Trajectory)
            {
                case ESpellTrajectory.Straight:
                    m_Controller.Animator.SetTrigger("CastShootStraight");
                    break;

                case ESpellTrajectory.Curve:
                    m_Controller.Animator.SetTrigger("CastShoot");
                    break;

                default:
                    Debug.LogError($"Trajectory {spellData.Trajectory} not implemented");
                    break;
            }

            m_Controller.Animator.SetFloat("CastSpeed", 1 / spellData.AnimationTimer);
        }

        /// <summary>
        /// Trigger end of animation of the spell
        /// </summary>
        void EndAnimator()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);
            m_Controller.Animator.SetTrigger("CancelCast");

            switch (spellData.Trajectory)
            {
                case ESpellTrajectory.Curve:
                case ESpellTrajectory.Straight:
                case ESpellTrajectory.Hight:
                    break;

                default:
                    Debug.LogError($"Trajectory {spellData.Trajectory} not implemented");
                    break;
            }

            // reset speed of animation
            m_Controller.Animator.SetFloat("CastSpeed", 1f);
        }

        /// <summary>
        /// Start animation particles if any
        /// </summary>
        void StartAnimationParticles()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);

            if (spellData.AnimationParticles == null)
                return;

            m_AnimationParticles = GameObject.Instantiate(spellData.AnimationParticles, m_SpellSpawn);
        }

        /// <summary>
        /// Destroy animation particles if any
        /// </summary>
        void EndAnimationParticles()
        {
            if (m_AnimationParticles == null)
                return;
            Destroy(m_AnimationParticles);
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// Check if movement inputs have beed pressed
        /// </summary>
        void CheckActionExectution()
        {
            if (! m_Controller.IsPlayer)
                return;

            if (m_SelectedSpell == ESpells.Count)
                return;

            if (Input.GetMouseButtonDown(0) && IsTargettable())
            {
                DisplaySpellPreview();
            }

            if (Input.GetMouseButtonUp(0) && IsTargettable())
            {
                // get shoot position
                m_TargetPos.Value = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 0, 0);
                CastSelectedSpell();
            }

            return;
        }

        /// <summary>
        /// Select the given spell
        /// </summary>
        /// <param name="spellType"></param>
        void SelectSpell(ESpells spellType)
        {
            m_SelectedSpell = spellType;
        }

        /// <summary>
        /// display the preview of where the spell will land
        /// </summary>
        /// <param name="spellType"></param>
        void DisplaySpellPreview()
        {
            if (m_SelectedSpell == ESpells.Count)
                return;

            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);
            spellData.SpellPreview(m_TargettableArea, m_SpellSpawn, GetSpawnOffset(spellData.Trajectory));
        }

        /// <summary>
        /// Cast the given spell
        /// </summary>
        /// <param name="spell"></param>
        void CastSelectedSpell()
        {
            if (m_SelectedSpell == ESpells.Count)
                return;
            StartCoroutine(StartCast(m_SelectedSpell));
        }

        #endregion


        #region Public Manipulators

        /// <summary>
        /// Check if click was in an targettable area
        /// </summary>
        /// <returns></returns>
        public bool IsTargettable()
        {
            return SpellHandler.IsTargettable(m_TargettableArea);
        }

        /// <summary>
        /// Check if click was in an targettable area
        /// </summary>
        /// <returns></returns>
        public static bool IsTargettable(Transform targettableArea)
        {
            var MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RectTransform targettableAreaRect = targettableArea.GetComponent<RectTransform>();

            return MousePosition.x > targettableArea.position.x - targettableAreaRect.rect.width / 2
                && MousePosition.x < targettableArea.position.x + targettableAreaRect.rect.width / 2
                && MousePosition.y > targettableArea.position.y
                && MousePosition.y < targettableArea.position.y + targettableAreaRect.rect.height;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public void SetCooldown(ESpells spellType, float cooldown)
        {
            if (! IsOwner)
                return;

            if (cooldown < 0f)
                cooldown = 0f;
            Cooldowns[GetSpellIndex(spellType)] = cooldown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public float GetCooldown(ESpells spellType)
        {
            return Cooldowns[GetSpellIndex(spellType)];
        }

        /// <summary>
        /// Get the index if the spell in list of index
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public int GetSpellIndex(ESpells spellType)
        {
            if (!m_Spells.Contains(spellType))
            {
                ErrorHandler.FatalError($"SpellHandler : spell {spellType} was not found in list of spells");
                return 0;
            }

            return m_Spells.IndexOf(spellType);
        }

        /// <summary>
        /// Cast the given spell
        /// </summary>
        /// <param name="spell"></param>
        public void AskSpellSelection(ESpells spellType)
        {
            if (!CanCast(spellType))
                return;

            SelectSpell(spellType);
        }

        /// <summary>
        /// Check if the given spell can be cast
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public bool CanCast(ESpells spellType)
        {
            return GetCooldown(spellType) <= 0f;
        }

        /// <summary>
        /// Calculate offset of the spawn position depending on the trajectory
        /// </summary>
        /// <param name="trajectory"></param>
        /// <returns></returns>
        Vector3 GetSpawnOffset(ESpellTrajectory trajectory)
        {
            int rotationFactor = m_Controller.transform.rotation.y > 0 ? 1 : -1;

            switch (trajectory)
            {
                case ESpellTrajectory.Straight:
                    return new Vector3(0, 0, 0);

                case ESpellTrajectory.Curve:
                    return new Vector3(rotationFactor * 0.1f, 0.25f, 0);

                case ESpellTrajectory.Hight:
                    return new Vector3(rotationFactor * m_SpellSpawn.transform.position.x, 1f, 0); ;

                default:
                    ErrorHandler.FatalError($"Trajectory {trajectory} not implemented");
                    return new Vector3(0, 0, 0);
            }
        }

        #endregion


        #region Coroutines 

        /// <summary>
        /// Cast the given spell
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        IEnumerator StartCast(ESpells spellType)
        {
            if (!IsOwner)
                yield break;

            // SETUP : get spell data and set animation to motion
            SpellData spellData = SpellLoader.GetSpellData(spellType);

            // cancel current movement
            m_Controller.Movement.CancelMovement(true);

            // reset rotation
            m_Controller.ResetRotation();

            // begin animation
            StartAnimationServerRPC();

            // set animation timer
            m_AnimationTimer.Value = SpellLoader.GetSpellData(m_SelectedSpell).AnimationTimer;
            
            // wait for spell to be casted during the animation
            while (m_AnimationTimer.Value >= spellData.AnimationTimer * (1 - spellData.CastAt))
            {
                m_AnimationTimer.Value -= Time.deltaTime;
                
                // if player is moving, cancel the spell
                if (m_Controller.Movement.IsMoving)
                {
                    // reset cancel current movement
                    m_Controller.Movement.CancelMovement(false);
                    // reset Animator
                    EndAnimationServerRPC();
                    yield break;
                }

                yield return null;
            }

            // ask server to cast the spell
            CastServerRPC();

            // wait for animation to finish (if not already)
            while (m_AnimationTimer.Value > 0f)
            {
                m_AnimationTimer.Value -= Time.deltaTime;
                // if player is moving, cancel the spell
                if (m_Controller.Movement.IsMoving)
                {
                    // reset cancel current movement
                    m_Controller.Movement.CancelMovement(false);
                    // reset Animator
                    EndAnimationServerRPC();
                    yield break;
                }

                yield return null;
            }

            // reset cancel current movement
            m_Controller.Movement.CancelMovement(false);
            // reset Animator
            EndAnimationServerRPC();
        }

        #endregion


        #region Getter / Setter Network

        ESpells m_SelectedSpell
        {
            get => (ESpells)m_SelectedSpellNet.Value;
            set => m_SelectedSpellNet.Value = (int)value;
        }

        List<ESpells> m_Spells
        {
            get
            {
                List<ESpells> spells = new List<ESpells>();
                foreach (int spellId in m_SpellsNet)
                    spells.Add((ESpells)spellId);
                
                return spells;
            }
        }

        #endregion
    }
}