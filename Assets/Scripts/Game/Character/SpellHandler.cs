using Assets.Scripts.Game;
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
        const float                         c_GlobalCooldown        = 0.3f; 

        // ===================================================================================
        // NETWORK VARIABLES
        /// <summary> list of cooldowns that links spellID to its cooldown <summary>
        NetworkList<float>                  m_CooldownsNet;
        /// <summary> list of spells that links spellID to spellValue <summary>
        NetworkList<int>                    m_SpellsNet;
        /// <summary> global cooldown when a spell is cast </summary>
        NetworkVariable<float>              m_GlobalCooldown        = new NetworkVariable<float>(0);
        /// <summary> currently selected spell </summary>
        NetworkVariable<int>                m_SelectedSpellNet      = new NetworkVariable<int>((int)ESpell.Count);
        /// <summary> time before the animation ends </summary>
        NetworkVariable<float>              m_AnimationTimer        = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        /// <summary> position where the spell will land </summary>
        NetworkVariable<Vector3>            m_TargetPos             = new NetworkVariable<Vector3>(default);

        // ===================================================================================
        // PRIVATE VARIABLES    
        /// <summary> owner's controller </summary>
        Controller                          m_Controller;
        /// <summary> base spawn position of the spell </summary>
        Transform                           m_SpellSpawn;

        // ===================================================================================
        // PUBLIC ACCESSORS
        public NetworkVariable<int>         SelectedSpellNet        => m_SelectedSpellNet;
        public NetworkList<float>           CooldownsNet            => m_CooldownsNet;
        public NetworkVariable<float>       AnimationTimer          => m_AnimationTimer;
        public bool                         IsCasting               => m_AnimationTimer.Value > 0f;
        public ESpell                       SelectedSpell           => m_SelectedSpell;
        public Transform                    SpellSpawn              => m_SpellSpawn;

        // ===================================================================================
        // EVENTS
        public Action<ESpell> OnSpellCasted;

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
        public void Initialize(List<ESpell> spells)
        {
            if (!IsServer)
                return;

            foreach (ESpell spell in spells)
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
        public void AskSpellSelectionServerRPC(ESpell spellType)
        {
            if (!CanSelect(spellType))
                return;

            SelectSpell(spellType);
        }

        /// <summary>
        /// Ask the server to select the given spell
        /// </summary>
        /// <param name="spell"></param>
        [ServerRpc]
        public void SetTargetPosServerRPC(Vector3 targetPos)
        {
            m_TargetPos.Value = targetPos;
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

            SpellCastedClientRPC(m_SelectedSpell);

            // only server can handle cooldowns and selected spell
            if (IsServer)
            {
                // setup global cooldown
                m_GlobalCooldown.Value = c_GlobalCooldown;

                // setup cooldown
                SetCooldown(m_SelectedSpell, spellData.Cooldown);

                // reset spell selection
                SelectSpell(Spells[0]);
            }
        }

        #endregion


        #region Client RPC

        [ClientRpc]
        void SpellCastedClientRPC(ESpell spell)
        {
            OnSpellCasted?.Invoke(spell);
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// Update cooldowns
        /// </summary>
        void UpdateCooldowns()
        {
            if (m_GlobalCooldown.Value > 0f)
                m_GlobalCooldown.Value -= Time.deltaTime;

            foreach (ESpell spell in Spells)
            {
                if (GetCooldown(spell) <= 0f)
                    continue;
                SetCooldown(spell, GetCooldown(spell) - Time.deltaTime);
            }
        }

        /// <summary>
        /// Check if movement inputs have beed pressed
        /// </summary>
        void CheckActionExectution()
        {
            // if no spell is selected : return
            if (m_SelectedSpell == ESpell.Count)
                return;

            // if unable to cast : return
            if (! CanCast(m_SelectedSpell))
                return;

            if (Input.GetMouseButtonDown(0) && IsTargettable())
            {
                DisplaySpellPreview();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (! IsInstantSpell && ! IsTargettable())
                    return;

                // get shoot position
                SetTargetPosServerRPC(new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 0, 0));
                CastSelectedSpell();
            }

            return;
        }

        /// <summary>
        /// Select the given spell
        /// </summary>
        /// <param name="spellType"></param>
        void SelectSpell(ESpell spellType)
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
            if (m_SelectedSpell == ESpell.Count)
                return;

            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);
            spellData.SpellPreview(TargettableArea, m_SpellSpawn, GetSpawnOffset(spellData.Trajectory));
        }

        /// <summary>
        /// Cast the given spell
        /// </summary>
        /// <param name="spell"></param>
        void CastSelectedSpell()
        {
            if (m_SelectedSpell == ESpell.Count)
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
            return SpellHandler.IsTargettable(TargettableArea);
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
        public void SetCooldown(ESpell spellType, float cooldown)
        {
            // only server can change a cooldown value
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
        public float GetCooldown(ESpell spellType)
        {
            return m_CooldownsNet[GetSpellIndex(spellType)];
        }

        /// <summary>
        /// Get the index if the spell in list of index
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public int GetSpellIndex(ESpell spellType)
        {
            if (!Spells.Contains(spellType))
            {
                ErrorHandler.Warning($"SpellHandler : spell {spellType} was not found in list of spells");
                return 0;
            }

            return Spells.IndexOf(spellType);
        }

        /// <summary>
        /// Set timer to 0 to cancel the cast
        /// </summary>
        void CancelCast()
        {
            // reset cancel current movement
            m_Controller.Movement.CancelMovement(false);

            // set animation timer to 0
            m_AnimationTimer.Value = 0f;
        }

        /// <summary>
        /// Check if the given spell can be cast
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public bool CanSelect(ESpell spellType)
        {
            return GetCooldown(spellType) <= 0f;
        }

        /// <summary>
        /// Check if the given spell can be cast
        /// </summary>
        /// <param name="spellType"></param>
        /// <returns></returns>
        public bool CanCast(ESpell spellType)
        {
            return GetCooldown(spellType) <= 0f                     // spell on cooldown 
                && m_GlobalCooldown.Value <= 0f                     // global cooldown not done
                && ! m_Controller.StateHandler.IsStunned            // is stunned
                && ! m_Controller.CounterHandler.HasCounter         // is using a counter
                && ! IsCasting;                                     // is casting an other spell
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
        IEnumerator StartCast(ESpell spellType)
        {
            // only owner can ask for cast
            if (!IsOwner)
                yield break;

            if (IsCasting)
                CancelCast();

            // SETUP : get spell data and set animation to motion
            SpellData spellData = SpellLoader.GetSpellData(spellType);

            bool castDone = false;  

            // cancel current movement
            m_Controller.Movement.CancelMovement(true);

            // reset rotation
            m_Controller.ResetRotation();

            // set animation timer
            m_AnimationTimer.Value = spellData.AnimationTimer;
            
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
                if (m_Controller.Movement.IsMoving || m_Controller.StateHandler.IsStunned)
                {
                    // reset Animator
                    CancelCast();
                    yield break;
                }

                yield return null;
            }
        }

        #endregion


        #region Getter / Setter Network

        ESpell m_SelectedSpell
        {
            get => (ESpell)m_SelectedSpellNet.Value;
            set => m_SelectedSpellNet.Value = (int)value;
        }

        public List<ESpell> Spells
        {
            get
            {
                List<ESpell> spells = new List<ESpell>();
                foreach (int spellId in m_SpellsNet)
                    spells.Add((ESpell)spellId);
                
                return spells;
            }
        }

        public bool IsInstantSpell
        {
            get
            {
                SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);
                return spellData.SpellType == ESpellType.InstantSpell
                    || spellData.SpellType == ESpellType.Counter;
            }
        }

        public Transform TargettableArea
        {
            get
            {
                return ArenaManager.Instance.TargettableAreas[m_Controller.Team];
            }
        }

        #endregion
    }
}