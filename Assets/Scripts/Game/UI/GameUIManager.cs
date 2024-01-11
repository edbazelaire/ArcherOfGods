using Enums;
using Game.Character;
using Game.Managers;
using Game.Spells;
using Game.UI;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    static GameUIManager s_Instance;

    const string        c_PlayerUIContainerPrefix   = "PlayerUIContainer_";
    const string        c_SpellsContainer           = "SpellsContainer";
    const int           NUM_TEAMS                   = 2;

    /// <summary> Template of a PlayerUI to create on Character Instantiation </summary>
    public GameObject   m_PlayerUITemplate;
    /// <summary> Spell item template to instantiate on Character Instantiation </summary>
    public GameObject   SpellTemplate;

    GameObject          m_SpellContainer;
    List<GameObject>    m_PlayerUIContainers;
    List<SpellItemUI>   m_SpellItems;   

    public GameObject SpellContainer => m_SpellContainer;


    #region Initialization

    void Initialize()
    {
        m_SpellItems = new List<SpellItemUI>();

        FindPlayerUIContainers();
        FindSpellsContainer();
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
            if (!Checker.NotNull(container))
                continue;

            // remove all potential content in container
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }

            // add container as container for team "i"
            m_PlayerUIContainers.Add(container);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void FindSpellsContainer()
    {
        m_SpellContainer = GameObject.Find(c_SpellsContainer);
        if (!Checker.NotNull(m_SpellContainer))
            return;

        ClearSpells();
    }

    #endregion


    #region Public Manipulators

    /// <summary>
    /// Set the health bar for this controller
    /// </summary>
    /// <param name="healthBar"></param>
    public void SetPlayersUI(ulong ClientId, int team)
    {
        PlayerUI playerUI = Finder.FindComponent<PlayerUI>(Instantiate(m_PlayerUITemplate, m_PlayerUIContainers[team].transform));
        playerUI.Initialize(ClientId);
    }

    /// <summary>
    /// add a SpellItemUI to the spell container
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="spell"></param>
    public void CreateSpellTemplate(ESpell spell)
    {
        SpellItemUI spellItem = Finder.FindComponent<SpellItemUI>(GameObject.Instantiate(SpellTemplate, m_SpellContainer.transform));
        spellItem.Initialize(spell);
        m_SpellItems.Add(spellItem);
    }

    /// <summary>
    /// remove all SpellItemUI from the spell container
    /// </summary>
    public void ClearSpells()
    {
        foreach (Transform child in m_SpellContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

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
