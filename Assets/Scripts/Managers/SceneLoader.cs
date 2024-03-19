using Assets;
using Assets.Scripts.UI;
using Enums;
using Game;
using Network;
using System;
using System.Collections;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour
{ 
    static SceneLoader s_Instance;

    [SerializeField] LoadingScreen m_LoadingScreen;

    string m_SceneLoading = "";
    public string SceneLoading => m_SceneLoading;
    public bool IsLoading => m_SceneLoading != "";

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadScene(string sceneName)
    {
        // can not load scene while an other scene is loading
        if (m_SceneLoading != "")
        {
            ErrorHandler.Warning("Trying to load scene " + sceneName + " while a scene is already loading : " + m_SceneLoading);
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        m_SceneLoading = sceneName;

        Main.SetState(EAppState.LoadingScreen);

        if (m_LoadingScreen == null)
            ErrorHandler.FatalError("Loading screen not set");

        m_LoadingScreen.Display(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        float progress;
        float nSteps = sceneName == "Arena" ? 2f : 1f;

        while (!asyncLoad.isDone)
        {
            progress = Mathf.Clamp01(asyncLoad.progress / (nSteps * 0.9f));
            m_LoadingScreen.SetProgress(progress);
            yield return null;
        }

        // call actions that happens when a scene is done loading 
        OnSceneLoaded();

        // when laoding arena, the switch to Game State
        if (sceneName == "Arena")
        {
            Main.SetState(EAppState.InGame);
        }
        else
        {
            if (!Enum.TryParse(sceneName, out EAppState appState))
                ErrorHandler.FatalError("Unable to find scene " + sceneName + " as EAppState");

            Main.SetState(appState);
        }

        switch (sceneName)
        {
            case "Arena":
                // waiting for GameManager to be created
                while (! GameManager.Exists)
                {
                    yield return null;
                }

                while (LobbyHandler.Instance.State <= ELobbyState.WaitingGameManager)
                {
                    yield return null;
                }

                while (! GameManager.Instance.IsGameStarted)
                {
                    progress = 1/nSteps + GameManager.Instance.ProgressGameStart.Value / nSteps;
                    m_LoadingScreen.SetProgress(Mathf.Clamp01(progress));
                    yield return null;
                }
                break;
        }

        m_LoadingScreen.Display(false);
    }

    /// <summary>
    /// Reset name, setup MainCanvas with MainCamera
    /// </summary>
    void OnSceneLoaded()
    {
        Main.Canvas.worldCamera = Camera.main;
        m_SceneLoading = "";
    }

    public static SceneLoader Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindFirstObjectByType<SceneLoader>();
                if (s_Instance == null)
                    ErrorHandler.FatalError("Unable to find SceneLoader");

                s_Instance.m_LoadingScreen.Display(false);
            }
            return s_Instance;
        }
    }
}
