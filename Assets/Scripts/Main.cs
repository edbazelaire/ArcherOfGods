using Assets.Scripts.Network;
using Enums;
using Game.Managers;
using Managers;
using Menu.PopUps;
using Save;
using System;
using System.Collections;
using System.Threading.Tasks;
using Tools;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Data;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets
{
    public class Main : MonoBehaviour
    {
        #region Members

        static Main s_Instance;

        [SerializeField] Canvas m_Canvas;
        [SerializeField] CloudSaveManager m_CloudSaveManager;
        [SerializeField] float m_CharacterSizeFactor = 1.0f;
        [SerializeField] float m_SpellSizeFactor = 0.3f;    

        public static event Action<EAppState>   StateChangedEvent;
        public static event Action              InitializationCompletedEvent;
        public static event Action              ApplicationQuitEvent;

        EAppState       m_State                 = EAppState.Release;
        bool            m_SignedIn              = false;
        
        public static Main Instance                     => s_Instance;
        public static CloudSaveManager CloudSaveManager => Instance.m_CloudSaveManager;
        public static EAppState State                   => Instance.m_State;
        public static Canvas Canvas                     => Instance.m_Canvas;
        public static float CharacterSizeFactor         => Instance.m_CharacterSizeFactor;
        public static float SpellSizeFactor             => Instance.m_SpellSizeFactor;

        #endregion


        #region Initialization 

        // Use this for initialization
        async void Awake()
        {
            s_Instance = this;

            await Initialize();

            DontDestroyOnLoad(this);
            DontDestroyOnLoad(m_Canvas);
        }

        async Task Initialize()
        {
            try
            {
                // register to state changes 
                StateChangedEvent += OnStateChanged;

                // register on initliazitation completed check by the Coroutine
                InitializationCompletedEvent += OnInitializationCompleted;

                StartCoroutine(CheckInitialization());

                // initialize UnituServices
                await UnityServices.InitializeAsync();

                // listen to Auth Service and try to signe in anonymously
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                AuthenticationService.Instance.SignedIn += m_CloudSaveManager.LoadSave;
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            } catch (Exception ex)
            {
                ErrorHandler.Error(ex.Message);
            }

#if UNITY_EDITOR
                Debug.Log("UNITY EDITOR MODE");
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            ApplicationQuitEvent += m_CloudSaveManager.OnApplicationQuit;
#endif
        }

        /// <summary>
        /// Coroutine checking that avery component of the app are propertly loaded
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckInitialization()
        {
            float timer = 30f; // set a timer of 30s to avoid inf loop
            while (
                CharacterLoader.Instance == null
                || SpellLoader.Instance == null
                || SceneLoader.Instance == null
                || RelayHandler.Instance == null
                || ! m_CloudSaveManager.LoadingCompleted
                || ! m_SignedIn
            )
            {
                timer -= Time.deltaTime;

                if (timer <= 0)
                    ErrorHandler.FatalError("Unable to initilaize the App in less than 30 seconds");

                yield return null;
            }

            InitializationCompletedEvent?.Invoke();
        }

        #endregion

        
        #region State Management

        public static void SetState(EAppState state)
        {
            if (state == Instance.m_State) 
                return;

            Instance.m_State = state;

            StateChangedEvent?.Invoke(state);
        }

        #endregion


        #region PopUp & Screens

        public static void SetPopUp(EPopUpState popUpState, params object[] args)
        {
            var popUpPath = "";
            Transform parent = Instance.m_Canvas.transform;
            if (popUpState.ToString().EndsWith("Screen"))
            {
                popUpPath = AssetLoader.c_OverlayPath;
            } else if (popUpState.ToString().EndsWith("PopUp"))
            {
                popUpPath = AssetLoader.c_PopUpsPath;
                parent = null;
            } else
            {
                ErrorHandler.Error("Unknown PopUp type " + popUpState.ToString() + " : unable to find adequate path");
            }

            // instantiate object in the canvas
            var obj = Instantiate(AssetLoader.Load<GameObject>(popUpPath + popUpState.ToString()), parent);
            if (obj == null)
            {
                ErrorHandler.Error("Unable to find popup : " + popUpState.ToString());
                return;
            }

            // setup initalization depending on the popup state
            switch (popUpState) {
                case EPopUpState.ChestOpeningScreen:
                    EChestType chestType = (EChestType)args[0];
                    int chestIndex = args.Length > 1 ? (int)args[1] : -1;
                    obj.GetComponent<ChestOpeningScreen>().Initialize(chestType, chestIndex);
                    break;

                case EPopUpState.LevelUpScreen:
                    ECharacter character = (ECharacter)args[0];
                    obj.GetComponent<LevelUpScreen>().Initialize(character);
                    break;

                case EPopUpState.SpellInfoPopUp:
                    ESpell spell = (ESpell)args[0];
                    int level = (int)args[1];
                    obj.GetComponent<SpellInfoPopUp>().Initialize(spell, level);
                    break;

                case EPopUpState.StateEffectPopUp:
                    obj.GetComponent<StateEffectPopUp>().Initialize((SStateEffectData)args[0], (int)args[1]);
                    break;

                case EPopUpState.RuneSelectionPopUp:
                    obj.GetComponent<RuneSelectionPopUp>().Initialize();
                    break;

                default:
                    obj.GetComponent<OverlayScreen>().Initialize();
                    break;
            }
        }

        public static void ErrorMessagePopUp(string message)
        {
            // TODO : error animation

            // TODO : error message
            Debug.LogWarning(message);
            //SetPopUp(EPopUpState.ErrorMessagePopUp, message);
        }

        public static void StateEffectPopUp(SStateEffectData stateEffectData, int level)
        {
            SetPopUp(EPopUpState.StateEffectPopUp, stateEffectData, level);
        }

        #endregion


        #region Listeners

        void OnSignedIn()
        {
            m_SignedIn = true;
            StaticPlayerData.PlayerName = AuthenticationService.Instance.PlayerName ?? "SheepRapist";
        }

        private void OnInitializationCompleted()
        {
            if (SceneLoader.Instance == null)
                Debug.Log("SceneLoader is null");

            Debug.Log("Initialization of the data completed : loading MainMenu");
            SceneLoader.Instance.LoadScene("MainMenu");
        }

        private void OnStateChanged(EAppState state)
        {
            Debug.Log("New state : " + state.ToString());
        }

#if UNITY_EDITOR
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // call manual event that the application is quitting
            if (state == PlayModeStateChange.ExitingPlayMode)
                ApplicationQuitEvent?.Invoke();
        }
#endif
        #endregion
    }
}