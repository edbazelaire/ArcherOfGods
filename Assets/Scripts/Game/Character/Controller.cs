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
    // -- Data
    ECharacter              m_Character;
    int                     m_Team;
    bool                    m_IsPlayer;

    // -- Components & GameObjects
    AnimationHandler        m_AnimationHandler;
    Movement                m_Movement;
    Life                    m_Life;
    SpellHandler            m_SpellHandler;
    HealthBar               m_HealthBar;
    GameObject              m_CharacterPreview;

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

    #endregion


    #region Initialization 

    /// <summary>
    /// Called when the controller is spawned on the network
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // init the controller
        Initialize(ECharacter.Reaper, GameManager.Instance.Players.Count, true);

        // add player to the game manager
        GameManager.Instance.AddPlayer(this);

        // display the player's ui 
        SetupUI();

        // ask the Server to select first spell by default
        if (IsOwner)
            m_SpellHandler.AskSpellSelectionServerRPC(m_SpellHandler.Spells[0]);
    }
 
    /// <summary>
    /// Initialize the controller
    /// </summary>
    public void Initialize(ECharacter character, int team, bool isPlayer)
    {
        m_Character         = character;
        m_Team              = team;
        m_IsPlayer          = isPlayer;

        m_Life              = Finder.FindComponent<Life>(gameObject);
        m_Movement          = Finder.FindComponent<Movement>(gameObject);
        m_SpellHandler      = Finder.FindComponent<SpellHandler>(gameObject);
        m_AnimationHandler  = Finder.FindComponent<AnimationHandler>(gameObject);
        
        // initialize with the character data
        InitializeCharacterData();
        
        // setup postion and rotation
        transform.position  = GameManager.Instance.Spawns[Team][0].position;
        ResetRotation();
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
        m_AnimationHandler.Initialize(Finder.FindComponent<Animator>(m_CharacterPreview));

        // initialize SpellHandler with character's spells
        m_SpellHandler.Initialize(characterData.Spells);
    }

    /// <summary>
    /// Setup the UI for this controller
    ///     + [Local] Create a SpellItemUI for each spell of the character
    ///     + [Global] Add a health bar for this controller
    /// </summary>
    public void SetupUI()
    {
        SetupSpellUI();
        SetHealthBar(GameUIManager.Instance.CreateHealthBar(Team));
    }

    /// <summary>
    /// Set the health bar for this controller
    /// </summary>
    /// <param name="healthBar"></param>
    public void SetHealthBar(HealthBar healthBar)
    {
        m_HealthBar = healthBar;
        m_HealthBar.SetMaxHealth(m_Life.InitialHp);

        m_Life.Hp.OnValueChanged += m_HealthBar.SetHealth;
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
        foreach (ESpells spell in CharacterLoader.GetCharacterData(Character).Spells)
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

    /// <summary>
    /// Kill the character
    /// </summary>
    public void Die()
    {
        m_CharacterPreview.SetActive(false);
        m_Movement.enabled = false;
        m_SpellHandler.enabled = false;
        GameUIManager.Instance.enabled = false;
    }

    #endregion

}
