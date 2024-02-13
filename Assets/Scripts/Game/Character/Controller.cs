using Data;
using Enums;
using Game;
using Game.Character;
using Game.Managers;
using Tools;
using Unity.Netcode;
using UnityEngine;

public class Controller : NetworkBehaviour
{
    #region Members      

    // ===================================================================================
    // PRIVATE VARIABLES 
    // -- Network Variables
    NetworkVariable<ECharacter>     m_Character     = new NetworkVariable<ECharacter>(ECharacter.Count);
    NetworkVariable<int>            m_Team          = new NetworkVariable<int>(-1);
    NetworkVariable<bool>           m_IsPlayer      = new NetworkVariable<bool>(true);
    NetworkVariable<bool>           m_IsInitialized = new NetworkVariable<bool>(false);

    // -- Components & GameObjects
    GameObject              m_CharacterPreview;
    AnimationHandler        m_AnimationHandler;
    Movement                m_Movement;
    Life                    m_Life;
    EnergyHandler           m_EnergyHandler;
    SpellHandler            m_SpellHandler;
    StateHandler            m_StateHandler;
    CounterHandler          m_CounterHandler;
    Collider2D              m_Collider;     

    // ===================================================================================
    // PUBLIC ACCESSORS

    // -- Data
    public ECharacter       Character           => m_Character.Value;
    public int              Team                => m_Team.Value;
    public bool             IsPlayer            => m_IsPlayer.Value;
    public ulong            PlayerId            => IsPlayer ? OwnerClientId : GameManager.POUTCH_CLIENT_ID;


    // -- Components & GameObjects
    public GameObject       CharacterPreview    => m_CharacterPreview;
    public AnimationHandler AnimationHandler    => m_AnimationHandler;
    public Movement         Movement            => m_Movement;
    public Life             Life                => m_Life;
    public SpellHandler     SpellHandler        => m_SpellHandler;
    public StateHandler     StateHandler        => m_StateHandler;
    public CounterHandler   CounterHandler      => m_CounterHandler;
    public EnergyHandler    EnergyHandler       => m_EnergyHandler;
    public Collider2D       Collider            => m_Collider;


    #endregion


    #region Initialization 

    /// <summary>
    /// Called when the controller is spawned on the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Debug.Log("Controller.OnNetworkSpawn()");   

        m_Collider = Finder.FindComponent<Collider2D>(gameObject);

        // setup components
        m_Life              = Finder.FindComponent<Life>(gameObject);
        m_EnergyHandler     = Finder.FindComponent<EnergyHandler>(gameObject);
        m_Movement          = Finder.FindComponent<Movement>(gameObject);
        m_SpellHandler      = Finder.FindComponent<SpellHandler>(gameObject);
        m_AnimationHandler  = Finder.FindComponent<AnimationHandler>(gameObject);
        m_StateHandler      = Finder.FindComponent<StateHandler>(gameObject);
        m_CounterHandler    = Finder.FindComponent<CounterHandler>(gameObject);

        // add event to call UI initialization after NetworkVariable update 
        m_IsInitialized.OnValueChanged += OnInitializedChanged;
    }


    void OnInitializedChanged(bool old, bool newValue)
    {
        Debug.LogWarning("==============================================================");
        Debug.Log("Initialized : ");
        Debug.Log("     + LocalClient : " + NetworkManager.Singleton.LocalClientId);
        Debug.Log("     + Owner : " + OwnerClientId);
        Debug.LogWarning("==============================================================");

        // add controller on client side
        if (newValue)
            GameManager.Instance.AddController(PlayerId, this);
    }

    /// <summary>
    /// Initialize the controller
    /// </summary>
    public void Initialize(ECharacter character, int team, bool isPlayer = true)
    {
        if (!IsServer)
            return;

        m_Character.Value       = character;
        m_Team.Value            = team;
        m_IsPlayer.Value        = isPlayer;

        Life.Hp.OnValueChanged += OnHpChanged;

        transform.position = ArenaManager.Instance.Spawns[team][0].position;
        transform.rotation = Quaternion.Euler(0f, team == 0 ? 0f : -180f, 0f);

        InitializeCharacterData(character);

        m_IsInitialized.Value = true;
    }

    public void InitializeUI()
    {
        ECharacter character    = m_Character.Value;
        int team                = m_Team.Value;

        // display the player's ui 
        GameUIManager.Instance.SetPlayersUI(PlayerId, team);

        CharacterData characterData = CharacterLoader.GetCharacterData(character);
        m_CharacterPreview = characterData.InstantiateCharacterPreview(gameObject);

        // setup local client position
        transform.localScale = transform.localScale * characterData.Size;

        // get animator
        Animator animator       = Finder.FindComponent<Animator>(m_CharacterPreview);
        m_AnimationHandler.Initialize(animator);

        // initialize base MovementSpeed
        m_AnimationHandler.UpdateMovementSpeed();

        // update personnal UI if is owner (and not an AI)
        if (!IsOwner || !IsPlayer)
            return;

        // flip camera for player of team 1
        if (team == 1)
        {
            Camera.main.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
            var cameraPos = Camera.main.transform.position;
            cameraPos.z *= -1;
            Camera.main.transform.position = cameraPos;
        }

        // setup the seplls icons buttons
        SetupSpellUI(character);

        // TODO
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

        // initialize MovementSpeed with character's speed
        m_Movement.Initialize(characterData.Speed);

        // init health and energy
        m_Life.Initialize(characterData.MaxHealth);
        m_EnergyHandler.Initialize(10, characterData.MaxEnergy);
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

    [ClientRpc]
    public void ActivateColliderClientRPC(bool on)
    {
        m_Collider.enabled = on;
    }

    #endregion


    #region Death & Game Over Manipulators

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
        ActivateActionComponent(false);
    }

    public void OnGameEnded(bool win)
    {
        // dead players have no end game method
        if (!m_Life.IsAlive)
            return;

        // call for 
        m_AnimationHandler.GameOverAnimation(win);

        // deactivate all "action" components
        ActivateActionComponent(false);
    }

    /// <summary>
    /// activate / deactivate players "action" components (that allows player to take actions)
    /// </summary>
    /// <param name="active"></param>
    void ActivateActionComponent(bool active)
    {
        m_Movement.enabled          = active;
        m_SpellHandler.enabled      = active;
        m_StateHandler.enabled      = active;
        m_CounterHandler.enabled    = active;
    }

    #endregion

}
