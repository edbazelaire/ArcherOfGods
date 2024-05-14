using Enums;
using Game;
using Game.Spells;
using Game.UI;
using Managers;
using Menu.Common.Buttons;
using Save;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    #region Members

    static GameUIManager s_Instance;

    [SerializeField] private IntroGameUI    m_IntroGameUI;
    [SerializeField] private EndGameUI      m_EndGameUI;

    const string        c_PlayerUIContainerPrefix   = "PlayerUIContainer_";
    const string        c_SpellsContainer           = "SpellsContainer";
    const int           NUM_TEAMS                   = 2;
    
    // ==============================================================================================================
    // Templates
    /// <summary> Template of a PlayerUI to create on Character Instantiation </summary>
    public GameObject   m_PlayerUITemplate;
    /// <summary> Spell item template to instantiate on Character Instantiation </summary>
    public GameObject   SpellTemplate;
    
    // ==============================================================================================================
    // Game Objects & Components
    /// <summary> button that inputs a movement to the left </summary>
    Button              m_LeftMovementButton;
    /// <summary> button that inputs a movement to the right </summary>
    Button              m_RightMovementButton;
    /// <summary> container for SpellItemUI(s) </summary>
    GameObject          m_SpellContainer;
    /// <summary> container for SpellItemUI(s) of linked spells </summary>
    GameObject          m_LinkedSpellsContainer;
    /// <summary> containers for PlayerUI(s) </summary>
    List<GameObject>    m_PlayerUIContainers;
    /// <summary> list of instantiated SpellItemUI(s) </summary>
    List<SpellItemUI>   m_SpellItems;

    // ==============================================================================================================
    // Data
    bool                m_LeftMovementButtonPressed;
    bool                m_RightMovementButtonPressed;

    // ==============================================================================================================
    // Public Accessors
    public static IntroGameUI IntroGameUI               => Instance.m_IntroGameUI;
    public static bool LeftMovementButtonPressed        => Instance.m_LeftMovementButtonPressed;
    public static bool RightMovementButtonPressed       => Instance.m_RightMovementButtonPressed;
   
    #endregion


    #region Initialization

    public void Initialize()
    {
        m_SpellItems = new List<SpellItemUI>();

        m_EndGameUI.gameObject.SetActive(false);

        FindPlayerUIContainers();
        FindMovementButtons();
        FindSpellsContainers();
    }

    /// <summary>
    /// 
    /// </summary>
    void FindPlayerUIContainers()
    {
        m_PlayerUIContainers = new List<GameObject>();
        for (int i = 0; i < NUM_TEAMS; i++)
        {
            // get container game object
            GameObject container = GameObject.Find(GetPlayerUIContainerName(i));

            // remove all childs in container
            UIHelper.CleanContent(container);

            // add container as container for team "i"
            m_PlayerUIContainers.Add(container);
        }
    }

    /// <summary>
    /// Get movement buttons
    /// </summary>
    void FindMovementButtons()
    {
        var container           = Finder.Find(gameObject, "MovementButtonsContainer");
        m_LeftMovementButton    = Finder.FindComponent<Button>(container.gameObject, "LeftMovementButton");
        m_RightMovementButton   = Finder.FindComponent<Button>(container.gameObject, "RightMovementButton");

        // link pressed button bools to pressed envents
        Finder.FindComponent<HoldOnButton>(m_LeftMovementButton.gameObject).PressedEvent += (bool pressed) => { m_LeftMovementButtonPressed = pressed; };
        Finder.FindComponent<HoldOnButton>(m_RightMovementButton.gameObject).PressedEvent += (bool pressed) => { m_RightMovementButtonPressed = pressed; };
    }

    /// <summary>
    /// 
    /// </summary>
    void FindSpellsContainers()
    {
        m_SpellContainer = Finder.Find(gameObject, c_SpellsContainer);
        m_LinkedSpellsContainer = Finder.Find(gameObject, "LinkedSpellsContainer");

        ClearSpells();
    }


    void DeleteGameUI()
    {
        Destroy(gameObject);
    }

    #endregion


    #region Public Manipulators

    /// <summary>
    /// Set the health bar for this controller
    /// </summary>
    public void SetPlayersUI(ulong ClientId, int team)
    {
        bool ally = GameManager.Instance.Owner.Team == team;

        PlayerUI playerUI = Finder.FindComponent<PlayerUI>(Instantiate(m_PlayerUITemplate, m_PlayerUIContainers[ally ? 0 : 1].transform));
        playerUI.Initialize(ClientId);
    }

    /// <summary>
    /// add a SpellItemUI to the spell container
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="spell"></param>
    public void CreateSpellTemplate(ESpell spell, int level)
    {
        SpellItemUI spellItem = Finder.FindComponent<SpellItemUI>(GameObject.Instantiate(SpellTemplate, m_SpellContainer.transform));
        spellItem.Initialize(spell, level);
        m_SpellItems.Add(spellItem);
    }

    /// <summary>
    /// add a SpellItemUI to the spell container
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="spell"></param>
    public void CreateLinkedSpellTemplate(ESpell spell, int level)
    {
        SpellItemUI spellItem = Finder.FindComponent<SpellItemUI>(GameObject.Instantiate(SpellTemplate, m_LinkedSpellsContainer.transform));
        spellItem.Initialize(spell, level);
        m_SpellItems.Add(spellItem);
    }

    /// <summary>
    /// remove all SpellItemUI from the spell container
    /// </summary>
    public void ClearSpells()
    {
        UIHelper.CleanContent(m_SpellContainer);
        UIHelper.CleanContent(m_LinkedSpellsContainer);
    }

    /// <summary>
    /// Get the SpellItemUI of the given spell
    /// </summary>
    /// <param name="spell"></param>
    /// <returns></returns>
    public SpellItemUI GetSpellUIItem(ESpell spell) 
    {
        foreach (SpellItemUI spellItem in m_SpellItems)
        {
            if (spellItem.Spell == spell)
                return spellItem;
        }
        return null;
    }

    #endregion


    #region Intro UI

    public void SetUpIntroScreen()
    {
        if (m_IntroGameUI == null)
        {
            ErrorHandler.Error("IntroGameUI not provided");
            return;
        }

        Dictionary<ulong, SPlayerData> playerData = FetchPlayerData();

        m_IntroGameUI.Initialize(playerData);
    }

    Dictionary<ulong, SPlayerData> FetchPlayerData()
    {
        var playerDataDict = new Dictionary<ulong, SPlayerData>();
        foreach (Controller controller in GameManager.Instance.Controllers.Values)
        {
            playerDataDict.Add(controller.PlayerId, controller.PlayerData);
        }

        return playerDataDict;
    }

    #endregion


    #region GameOver

    public void SetUpGameOver(bool win)
    {
        m_EndGameUI.gameObject.SetActive(true);
        m_EndGameUI.SetUpGameOver(win);

        // destroy self
        DeleteGameUI();
    }

    #endregion


    #region Static Members

    public static GameUIManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindFirstObjectByType<GameUIManager>();
                if (s_Instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(GameUIManager).Name;
                    s_Instance = obj.AddComponent<GameUIManager>();
                }

                s_Instance.Initialize();
            }

            return s_Instance;
        }
    }

    public static string GetPlayerUIContainerName(int team)
    {
        return c_PlayerUIContainerPrefix + team;
    }

    #endregion
}
