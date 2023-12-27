using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    static GameUIManager s_Instance;

    const string c_HealthBarContainerPrefix = "HealthBarContainer_";
    const int NUM_TEAMS = 2;

    public GameObject HealthBar;

    List<GameObject> m_HealthBarContainers;


    #region Initialization

    void Initialize()
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

    #endregion


    #region Dependent Members

    public static GameUIManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType<GameUIManager>();
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
