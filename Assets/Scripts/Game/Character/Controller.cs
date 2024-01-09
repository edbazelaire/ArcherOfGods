using Assets.Scripts.Game.Character;
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
    // CONSTANTS
    const string c_CharacterPreview = "CharacterPreview";

    // ===================================================================================
    // PRIVATE VARIABLES 
    // -- Network Variables
    NetworkVariable<int>    m_CharacterNet = new NetworkVariable<int>((int)ECharacter.Count);

    // -- Data
    ECharacter m_Character;
    int                     m_Team;
    bool                    m_IsPlayer;

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
    public ECharacter       Character           => m_Character;
    public int              Team                => m_Team;
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

        m_CharacterNet.OnValueChanged += (int a, int b) => { Debug.Log(" m_CharacterNet.OnValueChanged : " + b); };

        //// ask the Server to select first spell by default
        //if (IsOwner)
        //    m_SpellHandler.AskSpellSelectionServerRPC(m_SpellHandler.Spells[0]);

        //// setup postion and rotation
        //transform.position = GameManager.Instance.Spawns[Team][0].position;

        //ResetRotation();
        //Life.Hp.OnValueChanged += OnHpChanged;

        //SetupSpellUI();
    }
 
    /// <summary>
    /// Initialize the controller
    /// </summary>
    public void Initialize(ECharacter character, int team, bool isPlayer)
    {
        Debug.Log("Controller.Initialize()");

        m_CharacterNet.Value    = (int)character;
        m_Character             = character;
        m_Team                  = team;
        m_IsPlayer              = isPlayer;

        // add player to the game manager
        //GameManager.Instance.AddPlayerServerRPC(this);

        // initialize with the character data
        InitializeCharacterData();

        // setup postion and rotation
        transform.position = GameManager.Instance.Spawns[Team][0].position;
        ResetRotation();

        Life.Hp.OnValueChanged += OnHpChanged;
    }

    public void InitializeUI()
    {
        // display the player's ui 
        GameUIManager.Instance.SetPlayersUI(OwnerClientId, Team);

        // setup the seplls icons buttons
        SetupSpellUI();

        // ask the Server to select first spell by default
        if (IsOwner)
            m_SpellHandler.AskSpellSelectionServerRPC(m_SpellHandler.Spells[0]);
    }

    /// <summary>
    /// Implement all data related to the Character
    /// </summary>
    void InitializeCharacterData()
    {
        CharacterData characterData = CharacterLoader.GetCharacterData(Character);

        // instantiate the character preview
        m_CharacterPreview = characterData.InstantiateCharacterPreview(gameObject);

        // get animator
        Animator animator = Finder.FindComponent<Animator>(m_CharacterPreview);
        m_AnimationHandler.Initialize(animator);

        // initialize SpellHandler with character's spells
        m_SpellHandler.Initialize(characterData.Spells);

        // init health and energy
        m_Life.Initialize(50);
        m_EnergyHandler.Initialize(10, 100);
    }

    /// <summary>
    /// Create the SpellItemUI for each spell of the character
    /// </summary>
    public void SetupSpellUI()
    {
        if (!IsOwner)
            return;

        // clear the spell container (in case any spell was already there)
        GameUIManager.Instance.ClearSpells();

        // create a SpellItemUI for each spell of the character
        foreach (ESpell spell in CharacterLoader.GetCharacterData(Character).Spells)
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
            m_Movement.DebugMessage();
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
