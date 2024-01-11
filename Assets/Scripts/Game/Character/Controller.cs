using Assets.Scripts.Game;
using Assets.Scripts.Game.Character;
using Data;
using Enums;
using Game.Character;
using Game.Managers;
using System;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Controller : NetworkBehaviour
{
    #region Members        //// ask the Server to select first spell by default


    // ===================================================================================
    // CONSTANTS
    const string c_CharacterPreview = "CharacterPreview";

    // ===================================================================================
    // PRIVATE VARIABLES 
    // -- Network Variables
    NetworkVariable<int>    m_CharacterNet = new NetworkVariable<int>((int)ECharacter.Count);

    // -- Data
    NetworkVariable<ECharacter>     m_Character = new (ECharacter.Count);
    NetworkVariable<int>            m_Team = new(0);
    bool                            m_IsPlayer;
    ECharacter m_CharacterValue     = ECharacter.Count;

    // -- Components & GameObjects
    GameObject              m_CharacterPreview;
    AnimationHandler        m_AnimationHandler;
    Movement                m_Movement;
    Life                    m_Life;
    EnergyHandler           m_EnergyHandler;
    SpellHandler            m_SpellHandler;
    StateHandler            m_StateHandler;
    CounterHandler          m_CounterHandler;

    // ===================================================================================
    // PUBLIC ACCESSORS

    // -- Data
    public ECharacter       Character           => m_Character.Value;
    public int              Team                => m_Team.Value;
    public bool             IsPlayer            => m_IsPlayer;

    // -- Components & GameObjects
    public GameObject       CharacterPreview    => m_CharacterPreview;
    public AnimationHandler AnimationHandler    => m_AnimationHandler;
    public Movement         Movement            => m_Movement;
    public Life             Life                => m_Life;
    public SpellHandler     SpellHandler        => m_SpellHandler;
    public StateHandler     StateHandler        => m_StateHandler;
    public CounterHandler   CounterHandler      => m_CounterHandler;
    public EnergyHandler    EnergyHandler       => m_EnergyHandler;


    public int test = 0;

    #endregion


    #region Initialization 

    /// <summary>
    /// Called when the controller is spawned on the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Debug.Log("Controller.OnNetworkSpawn()");   

        // setup components
        m_Life = Finder.FindComponent<Life>(gameObject);
        m_EnergyHandler = Finder.FindComponent<EnergyHandler>(gameObject);
        m_Movement = Finder.FindComponent<Movement>(gameObject);
        m_SpellHandler = Finder.FindComponent<SpellHandler>(gameObject);
        m_AnimationHandler = Finder.FindComponent<AnimationHandler>(gameObject);
        m_StateHandler = Finder.FindComponent<StateHandler>(gameObject);
        m_CounterHandler = Finder.FindComponent<CounterHandler>(gameObject);
    }
 
    /// <summary>
    /// Initialize the controller
    /// </summary>
    public void Initialize(ECharacter character, int team, bool isPlayer = true)
    {
        if (IsServer)
        {
            Debug.Log("Controller.Initialize()");
            Debug.Log("     + id " + OwnerClientId);
            Debug.Log("     + character " + character);
            Debug.Log("     + team " + team);

            m_CharacterNet.Value = (int)character;
            m_Character.Value = character;
            m_Team.Value = team;
            m_IsPlayer = isPlayer;

            Life.Hp.OnValueChanged += OnHpChanged;

            transform.position = ArenaManager.Instance.Spawns[team][0].position;
            transform.rotation = Quaternion.Euler(0f, team == 0 ? 0f : -180f, 0f);

            InitializeCharacterData(character);
        }
    }

    [ClientRpc]
    public void InitializeUIClientRPC(ECharacter character, int team)
    {
        // display the player's ui 
        GameUIManager.Instance.SetPlayersUI(OwnerClientId, team);

        CharacterData characterData = CharacterLoader.GetCharacterData(character);
        m_CharacterPreview = characterData.InstantiateCharacterPreview(gameObject);

        transform.position = ArenaManager.Instance.Spawns[team][0].position;
        transform.rotation = Quaternion.Euler(0f, team == 0 ? 0f : -180f, 0f);

        // get animator
        Animator animator = Finder.FindComponent<Animator>(m_CharacterPreview);
        m_AnimationHandler.Initialize(animator);

        if (!IsOwner)
            return;

        // setup the seplls icons buttons
        SetupSpellUI(character);
        //m_SpellHandler.AskSpellSelectionServerRPC(m_SpellHandler.Spells[0]);
    }

    /// <summary>
    /// Implement all data related to the Character
    /// </summary>
    void InitializeCharacterData(ECharacter character)
    {
        if (! IsServer)
            return;

        CharacterData characterData = CharacterLoader.GetCharacterData(character);

        // initialize SpellHandler with character's spells
        m_SpellHandler.Initialize(characterData.Spells);

        // init health and energy
        m_Life.Initialize(50);
        m_EnergyHandler.Initialize(10, 100);
    }

    /// <summary>
    /// Create the SpellItemUI for each spell of the character
    /// </summary>
    public void SetupSpellUI(ECharacter character)
    {
        if (!IsOwner)
            return;

        CharacterData characterData = CharacterLoader.GetCharacterData(character);

        // clear the spell container (in case any spell was already there)
        GameUIManager.Instance.ClearSpells();

        // create a SpellItemUI for each spell of the character
        foreach (ESpell spell in characterData.Spells)
        {
            GameUIManager.Instance.CreateSpellTemplate(spell);
        }
    }

    #endregion


    #region Server RPC


    #endregion


    #region Inherited Manipulators

    void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Client : " + NetworkManager.Singleton.LocalClientId);
            Debug.Log("     + Character " + m_Character.Value);
            Debug.Log("     + Team " + m_Team.Value);
            Debug.Log("     + Spells " + m_SpellHandler.Spells);
        }
    }


    #endregion


    #region Public Manipulators

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(0f, Team == 0 ? 0f : -180f, 0f);
    }

    #endregion


    #region Private Manipulators

    //void OnSpellSelected(int oldVale, int spell)
    //{
    //    Debug.Log("SpellSelected");
    //    GameUIManager.Instance.SelectSpell((ESpell)spell);
    //}

    //void OnCooldownChanged(NetworkListEvent<float> changeEvent)
    //{
    //    ESpell spell = m_SpellHandler.Spells[changeEvent.Index];

    //    float cooldown = changeEvent.Value;

    //    GameUIManager.Instance.ChangeCooldown(spell, cooldown);
    //}

    void OnHpChanged(int oldValue, int newValue)
    {
        if (newValue <= 0)
            Die();
    }

    /// <summary>
    /// Kill the character
    /// </summary>
    void Die()
    {
        m_CharacterPreview.SetActive(false);
        m_Movement.enabled = false;
        m_SpellHandler.enabled = false;
        m_StateHandler.enabled = false;
    }

    #endregion

}
