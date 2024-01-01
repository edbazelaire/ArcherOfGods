using Data;
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

        // ===================================================================================
        // CONSTANTS
        const string                        c_SpellSpawn            = "SpellSpawn";

        // ===================================================================================
        // EVENTS
        /// <summary> event fired when the spell is over </summary>
        public Action                       CastEndedEvent;

        // ===================================================================================
        // NETWORK VARIABLES    
        /// <summary> list of cooldowns that links spellID to its cooldown <summary>
        NetworkList<float>                  m_CooldownsNet;
        /// <summary> list of spells that links spellID to spellValue <summary>
        NetworkList<int>                    m_SpellsNet;
        /// <summary> currently selected spell </summary>
        NetworkVariable<int>                m_SelectedSpellNet      = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        /// <summary> time before the animation ends </summary>
        NetworkVariable<float>              m_AnimationTimer        = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        /// <summary> position where the spell will land </summary>
        NetworkVariable<Vector3>            m_TargetPos             = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // ===================================================================================
        // PRIVATE VARIABLES    
        /// <summary> owner's controller </summary>
        Controller                          m_Controller;
        /// <summary> zone where the player can cast a spell </summary>
        Transform                           m_TargettableArea;
        /// <summary> base spawn position of the spell </summary>
        Transform                           m_SpellSpawn;
        /// <summary> current animation particles displayed during animation </summary>
        GameObject                          m_AnimationParticles;

        // ===================================================================================
        // PUBLIC ACCESSORS
        public NetworkVariable<int>         SelectedSpellNet        => m_SelectedSpellNet;
        public NetworkList<float>           CooldownsNet            => m_CooldownsNet;
        public float                        AnimationTimer          => m_AnimationTimer.Value;
        public bool                         IsCasting               => m_AnimationTimer.Value > 0f;

        #endregion


        #region Inherited Manipulators

        private void Awake()
        {
            m_SpellsNet = new NetworkList<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            m_CooldownsNet   = new NetworkList<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        }

        public override void OnNetworkSpawn()
        {
            m_Controller = Finder.FindComponent<Controller>(gameObject);
            m_SpellSpawn = Finder.FindComponent<Transform>(gameObject, c_SpellSpawn);
            m_TargettableArea = GameManager.Instance.TargettableAreas[m_Controller.Team];   // get targettable area of the team
        }

        void Update()
        {
            // only server can update cooldowns
            if (IsServer)
                UpdateCooldowns();

            if (! IsOwner)
                return; 

            CheckActionExectution();
        }

        #endregion


        #region Initialization

        /// <summary>
        /// Initialize the spell handler
        /// </summary>
        /// <param name="spells"></param>
        public void Initialize(List<ESpells> spells)
        {
            if (!IsServer)
                return;

            foreach (ESpells spell in spells)
            {
                m_SpellsNet.Add((int)spell);
                m_CooldownsNet.Add(0);
            }
        }

        #endregion


        #region Server RPC

        /// <summary>
        /// Ask the server to select the given spell
        /// </summary>
        /// <param name="spell"></param>
        [ServerRpc]
        public void AskSpellSelectionServerRPC(ESpells spellType)
        {
            if (!CanCast(spellType))
                return;

            SelectSpell(spellType);
        }

        /// <summary>
        /// Ask the server to cast the selected spell
        /// </summary>
        [ServerRpc]
        void CastServerRPC()
        {
            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);
            
            // get spawn position and cast the spell
            var spawnPosition = m_SpellSpawn.position + GetSpawnOffset(spellData.Trajectory);
            spellData.Cast(m_Controller.OwnerClientId, m_TargetPos.Value, spawnPosition, m_SpellSpawn.rotation);

            // only server can handle cooldowns and selected spell
            if (IsServer)
            {
                // setup cooldown
                SetCooldown(m_SelectedSpell, spellData.Cooldown);

                // reset spell selection
                SelectSpell(Spells[0]);
            }
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
            // TODO : cancel current animation if any
            //EndAnimation();

            StartAnimator();
            StartAnimationParticles();
        }

        /// <summary>
        /// Reset animator on end/cancel of animation
        /// </summary>
        void EndAnimation()
        {
            // cancel animation
            if (IsCasting)
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
            // reset speed of animation
            m_Controller.Animator.SetTrigger("CancelCast");

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
            m_AnimationParticles = null;
        }

        #endregion


        #region Private Manipulators
        /// <summary>
        /// Update cooldowns
        /// </summary>
        void UpdateCooldowns()
        {
            foreach (ESpells spell in Spells)
            {
                SetCooldown(spell, GetCooldown(spell) - Time.deltaTime);
            }
        }

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
            if (! IsServer)
                return;
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
            if (! IsServer)
                return;

            if (cooldown < 0f)
                cooldown = 0f;

            m_CooldownsNet[GetSpellIndex(spellType)] = cooldown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public float GetCooldown(ESpells spellType)
        {
            return m_CooldownsNet[GetSpellIndex(spellType)];
        }

        /// <summary>
        /// Get the index if the spell in list of index
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public int GetSpellIndex(ESpells spellType)
        {
            if (!Spells.Contains(spellType))
            {
                ErrorHandler.FatalError($"SpellHandler : spell {spellType} was not found in list of spells");
                return 0;
            }

            return Spells.IndexOf(spellType);
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
            // only owner can ask for cast
            if (!IsOwner)
                yield break;

            // SETUP : get spell data and set animation to motion
            SpellData spellData = SpellLoader.GetSpellData(spellType);
            bool castDone = false;  

            // cancel current movement
            m_Controller.Movement.CancelMovement(true);

            // reset rotation
            m_Controller.ResetRotation();

            // begin animation
            StartAnimationServerRPC();

            // set animation timer
            m_AnimationTimer.Value = SpellLoader.GetSpellData(m_SelectedSpell).AnimationTimer;
            
            // wait for animation to finish (if not already)
            while (m_AnimationTimer.Value > 0f || ! castDone)
            {
                // if the spell is launched before the end of the cast : cast it
                if (m_AnimationTimer.Value <= spellData.AnimationTimer * (1 - spellData.CastAt) && ! castDone)
                {
                    // ask server to cast the spell
                    CastServerRPC();
                    castDone = true;
                }

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

        public List<ESpells> Spells
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