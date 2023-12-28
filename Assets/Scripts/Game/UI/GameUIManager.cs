using Game.Managers;
using Game.UI;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    static GameUIManager s_Instance;

    const string        c_HealthBarContainerPrefix  = "HealthBarContainer_";
    const string        c_SpellsContainer           = "SpellsContainer";
    const int           NUM_TEAMS                   = 2;

    /// <summary> Spell item template to instantiate on Character Instantiation </summary>
    public GameObject   SpellTemplate;
    /// <summary> Health bar template to instantiate on Character Instantiation </summary>
    public GameObject   HealthBar;

    GameObject          m_SpellContainer;
    List<GameObject>    m_HealthBarContainers;
    List<SpellItemUI>   m_SpellItems;   

    public GameObject SpellContainer => m_SpellContainer;


    #region Initialization

    void Initialize()
    {
        m_SpellItems = new List<SpellItemUI>();

        FindHealthbarContainers();
        FindSpellsContainer();
    }

    /// <summary>
    /// 
    /// </summary>
    void FindHealthbarContainers()
    {
        m_HealthBarContainers = new List<GameObject>();
        for (int i = 0; i < NUM_TEAMS; i++)
        {
            // get container game object
            GameObject container = GameObject.Find(GetHealthBarContainerName(i));
            if (!Checker.NotNull(container))
                continue;

            // remove all potential content in container
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }

            // add container as container for team "i"
            m_HealthBarContainers.Add(container);
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

        // remove all potential content in container
        foreach (Transform child in m_SpellContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion


    #region Public Manipulators

    /// <summary>
    /// Instantiate a health bar in the scene
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public HealthBar CreateHealthBar(int team)
    {
        // check team number inf to number of healthbar containers
        if (team > m_HealthBarContainers.Count)
        {
            ErrorHandler.Error($"number of containers ({m_HealthBarContainers.Count}) is less than provided team number ({team})");
            return null;
        }

        GameObject healthBar = Instantiate(HealthBar, m_HealthBarContainers[team].transform);
        return healthBar.GetComponent<HealthBar>();
    }

    public void CreateSpellTemplate(ESpells spell)
    {
        m_SpellItems.Add(new SpellItemUI(GameObject.Instantiate(SpellTemplate, m_SpellContainer.transform), spell));
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

    public static string GetHealthBarContainerName(int team)
    {
        return c_HealthBarContainerPrefix + team;
    }

    #endregion
}
