using Game.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader
{ 
    static SceneLoader s_Instance;
    static bool m_Initialized = false;

    private void Initialize()
    {
        Debug.Log("Initializing SceneLoader      ==================================================");

        while(! m_Initialized)
        {
            m_Initialized = true;

            if (SpellLoader.Instance == null)
            {
                m_Initialized = false;
                Debug.LogError("SpellLoader not ready");
            }

            if (CharacterLoader.Instance == null)
            {
                m_Initialized = false;
                Debug.LogError("CharacterLoader not ready");
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static SceneLoader Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new SceneLoader();
                s_Instance.Initialize();
            }
            return s_Instance;
        }
    }
}
