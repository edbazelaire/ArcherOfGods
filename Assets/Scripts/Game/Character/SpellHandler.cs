using Data;
using Enums;
using Game.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class SpellHandler : NetworkBehaviour
    {
        #region Members

        // ===================================================================================
        // CONSTANTS
        const string                        c_SpellSpawn            = "SpellSpawn";
        const float                         c_GlobalCooldown        = 0f; 

        // ===================================================================================
        // NETWORK VARIABLES       
        /// <summary> list of cooldowns that links spellID to its cooldown <summary>
        NetworkList<float>                  m_CooldownsNet;
        /// <summary> list of spells that links spellID to spellValue <summary>
        NetworkList<int>                    m_SpellsNet;
        /// <summary> list of spells that links spellID to spellValue <summary>
        NetworkList<int>                    m_SpellLevelsNet;
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
        /// <summary> spell that will be selected at the end of the current one </summary>
        ESpell                              m_NextSelectedSpell;
        /// <summary> is the player currently casting a spell ? </summary>
        bool                                m_IsCasting;     

        // ===================================================================================
        // PUBLIC ACCESSORS
        public NetworkVariable<int>         SelectedSpellNet        => m_SelectedSpellNet;
        public NetworkList<float>           CooldownsNet            => m_CooldownsNet;
        public NetworkVariable<float>       AnimationTimer          => m_AnimationTimer;
        public bool                         IsCasting               => m_IsCasting;
        public ESpell                       SelectedSpell           => m_SelectedSpell;
        public Transform                    SpellSpawn              => m_SpellSpawn;
        public Vector3                      TargetPos               => m_TargetPos.Value;   

        // ===================================================================================
        // EVENTS
        public Action<ESpell> OnSpellCasted;

        #endregion


        #region Inherited Manipulators

        private void Awake()
        {
            m_SpellsNet         = new NetworkList<int>(default);
            m_SpellLevelsNet    = new NetworkList<int>(default);
            m_CooldownsNet      = new NetworkList<float>(default);
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

        public override void OnNetworkDespawn()
        {
            //m_SpellsNet.Dispose();
            //m_SpellLevelsNet.Dispose();
            //m_CooldownsNet.Dispose();
        }

        #endregion


        #region Initialization

        /// <summary>
        /// Initialize the spell handler
        /// </summary>
        /// <param name="spells"></param>
        public void Initialize(ESpell autoAttack, ESpell ultimate, List<ESpell> extraSpells, List<int> spellLevels)
        {
            if (!IsServer)
                return;

            m_SpellsNet.Add((int)autoAttack);
            m_SpellLevelsNet.Add(m_Controller.CharacterLevel);
            m_CooldownsNet.Add(0);

            m_SpellsNet.Add((int)ultimate);
            m_SpellLevelsNet.Add(m_Controller.CharacterLevel);
            m_CooldownsNet.Add(0);

            for (int i=0; i < extraSpells.Count; i++)
            {
                m_SpellsNet.Add((int)extraSpells[i]);
                m_SpellLevelsNet.Add(spellLevels[i]);
                m_CooldownsNet.Add(0);
            }

            m_NextSelectedSpell = autoAttack;
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
            if (!IsServer)
                return;

            if (!CanSelect(spellType))
                return;

            SelectSpell(spellType);

            if (IsInstantSpell)
                CastSelectedSpell();
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
            Debug.Log("CastServerRPC() - start");
            SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell, m_SpellLevelsNet[GetSpellIndex(m_SelectedSpell)]);

            // get spawn position and cast the spell
            StartCoroutine(spellData.CastDelay(m_Controller.OwnerClientId, m_TargetPos.Value, m_SpellSpawn.position, m_SpellSpawn.rotation));

            // cast spell on client side
            SpellCastedClientRPC(m_SelectedSpell);

            // only server can handle cooldowns and selected spell
            if (IsServer)
            {
                // spend the energy of the spell
                if (spellData.EnergyCost > 0)
                    m_Controller.EnergyHandler.SpendEnergy(spellData.EnergyCost);

                // inform that casting is done
                m_IsCasting = false;

                // setup global cooldown
                m_GlobalCooldown.Value = c_GlobalCooldown;

                // setup cooldown
                SetCooldown(m_SelectedSpell, spellData.Cooldown);

                // reset spell selection
                SelectSpell(m_NextSelectedSpell);
            }

            Debug.Log("CastServerRPC() - end");
        }

        #endregion


        #region Client RPC

        [ClientRpc]
        void SpellCastedClientRPC(ESpell spell)
        {
            OnSpellCasted?.Invoke(spell);

            SpellLoader.GetSpellData(spell).SpawnOnCastPrefabs(m_Controller.transform, m_TargetPos.Value);
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

            if (IsCasting)
                m_NextSelectedSpell = spellType;
            else
            {
                // on spell selection, reset NextSelectedSpell to default auto attack
                m_SelectedSpell = spellType;
                m_NextSelectedSpell = (ESpell)m_SpellsNet[0];
            }
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
            spellData.SpellPreview(m_Controller);
        }

        /// <summary>
        /// Cast the given spell
        /// </summary>
        /// <param name="spell"></param>
        void CastSelectedSpell()
        {
            if (m_SelectedSpell == ESpell.Count)
                return;

            Debug.Log("Casting Spell : " + m_SelectedSpell);
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

            // stop casting
            m_IsCasting = false;
        }

        /// <summary>
        /// Check if the given spell can be selected (no cooldown and enought energy)
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public bool CanSelect(ESpell spell)
        {
            return GetCooldown(spell) <= 0f                         // spell not on cooldown 
                && SpellLoader.GetSpellData(spell).EnergyCost <= m_Controller.EnergyHandler.Energy.Value;    // check enought energy
        }

        /// <summary>
        /// Check if the given spell can be cast (no cooldown, enought energy, not doing a blocking action or in a state that prevents casts)
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public bool CanCast(ESpell spell)
        {
            return CanSelect(spell)
                && m_GlobalCooldown.Value <= 0f                                 // global cooldown done
                && ! HasStateBlockingCast()                                     // check state effect blocking the cast
                && ! IsCasting;                                                 // is not casting an other spell
        }

        public bool HasStateBlockingCast()
        {
            return m_Controller.StateHandler.IsStunned                        // is stunned
                && m_Controller.StateHandler.HasState(EStateEffect.Frozen)    // is frozen 
                && m_Controller.StateHandler.HasState(EStateEffect.Silence)   // is silenced 
                && m_Controller.CounterHandler.HasCounter;                     // is using a counter
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

            // if curently casting another spell, cancel it
            if (IsCasting)
                CancelCast();

            m_IsCasting = true;

            // SETUP : get spell data and set animation to motion
            SpellData spellData = SpellLoader.GetSpellData(spellType);

            // cancel current movement
            m_Controller.Movement.CancelMovement(true);

            // set animation timer
            m_AnimationTimer.Value = spellData.AnimationTimer;
            
            // wait for animation to finish (if not already)
            while (m_AnimationTimer.Value > 0f || m_IsCasting)
            {
                // if the spell is launched before the end of the cast : cast it
                if (m_AnimationTimer.Value <= spellData.AnimationTimer * (1 - spellData.CastAt) && m_IsCasting)
                {
                    // ask server to cast the spell
                    CastServerRPC();
                    m_Controller.Movement.CancelMovement(false);
                    m_IsCasting = false;
                }

                m_AnimationTimer.Value -= Time.deltaTime;

                // if player is moving, cancel the spell
                if ((spellData.IsCancellable && m_Controller.Movement.IsMoving) || HasStateBlockingCast())
                {
                    // reset Animator
                    CancelCast();
                    yield break;
                }

                yield return null;
            }

            m_IsCasting = false;
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

                try
                {
                    foreach (int spellId in m_SpellsNet)
                        spells.Add((ESpell)spellId);
                } catch (Exception ex) 
                {
                    ex.Equals(null);
                }
                
                return spells;
            }
        }

        public List<int> SpellLevels
        {
            get
            {
                List<int> levels = new List<int> ();
                foreach (int level in m_SpellLevelsNet)
                    levels.Add(level);
                
                return levels;
            }
        }

        public bool IsInstantSpell
        {
            get
            {
                SpellData spellData = SpellLoader.GetSpellData(m_SelectedSpell);
                return spellData.SpellType == ESpellType.InstantSpell
                    || spellData.SpellTarget == ESpellTarget.Self
                    || spellData.SpellTarget == ESpellTarget.FirstAlly
                    || spellData.SpellTarget == ESpellTarget.FirstEnemy;
            }
        }

        public Transform TargettableArea
        {
            get
            {
                return ArenaManager.Instance.EnemyTargettableArea;
            }
        }

        #endregion
    }
}