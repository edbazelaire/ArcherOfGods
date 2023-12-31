using Data;
using Enums;
using Game;
using Game.Character;
using Game.Managers;
using System;
using Tools;
using Unity.Netcode;
using UnityEngine;

public class Controller : NetworkBehaviour
{
    const string c_CharacterPreview = "CharacterPreview";

    public int              Id              { get; set; }
    public ECharacter       Character       { get; set; }
    public int              Team            { get; set; }
    public bool             IsCurrentPlayer { get; set; }
    public bool             IsPlayer        { get; set; }

    GameObject              m_CharacterPreview;

    Animator                m_Animator;
    Movement                m_Movement;
    Life                    m_Life;
    SpellHandler            m_SpellHandler;
    HealthBar               m_HealthBar;

    public GameObject       CharacterPreview    => m_CharacterPreview;
    public Animator         Animator            => m_Animator;
    public Movement         Movement            => m_Movement;
    public Life             Life                => m_Life;
    public SpellHandler     SpellHandler        => m_SpellHandler;


    #region Setup 

    public override void OnNetworkSpawn()
    {
        int team = GameManager.Instance.Players.Count;
        Initialize(0, ECharacter.GreenArcher, team, true, true);

        GameManager.Instance.AddPlayer(this);
    }
 
    /// <summary>
    /// Initialize the controller
    /// </summary>
    public void Initialize(int id, ECharacter character, int team, bool isPlayer, bool isCurrentPlayer)
    {
        Id              = id;
        Character       = character;
        Team            = team;
        IsPlayer        = isPlayer;
        IsPlayer        = isCurrentPlayer;

        m_CharacterPreview = Finder.Find(gameObject, c_CharacterPreview);

        m_Animator      = Finder.FindComponent<Animator>(m_CharacterPreview);
        m_Life          = Finder.FindComponent<Life>(gameObject);
        m_Movement      = Finder.FindComponent<Movement>(gameObject);
        m_SpellHandler  = Finder.FindComponent<SpellHandler>(gameObject);

        Checker.NotNull(m_Life);
        Checker.NotNull(m_Movement);    

        m_SpellHandler.Initialize(CharacterLoader.GetCharacterData(Character).Spells);
        SetHealthBar(GameUIManager.Instance.CreateHealthBar(team));
        SetupSpellUI();

        transform.position = GameManager.Instance.Spawns[Team][0].position;
        ResetRotation();
    }

    /// <summary>
    /// Set the health bar for this controller
    /// </summary>
    /// <param name="healthBar"></param>
    public void SetHealthBar(HealthBar healthBar)
    {
        m_HealthBar = healthBar;
        m_HealthBar.SetMaxHealth(m_Life.InitialHp);

        m_Life.HealthChangedEvent += m_HealthBar.SetHealth;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetupSpellUI()
    {
        if (!IsOwner)
            return;

        foreach (ESpells spell in CharacterLoader.GetCharacterData(Character).Spells)
        {
            GameUIManager.Instance.CreateSpellTemplate(this, spell);
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
