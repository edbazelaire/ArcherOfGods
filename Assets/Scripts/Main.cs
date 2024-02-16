using Assets.Scripts.Network;
using Enums;
using Game.Managers;
using Managers;
using Menu.PopUps;
using Save;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Tools;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Assets
{
    public class Main : MonoBehaviour
    {
        #region Members

        static Main s_Instance;

        [SerializeField] Canvas m_Canvas;
        [SerializeField] CloudSaveManager m_CloudSaveManager;

        public static event Action<EAppState>   StateChangedEvent;
        public static event Action              InitializationCompletedEvent;

        EAppState       m_State                 = EAppState.Release;
        bool            m_SignedIn              = false;
        
        public static Main Instance => s_Instance;
        public static CloudSaveManager CloudSaveManager => Instance.m_CloudSaveManager;
        public static EAppState State => Instance.m_State;
        public static Canvas Canvas => Instance.m_Canvas;

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
            // instantiate object in the canvas
            var obj = Instantiate(AssetLoader.Load<GameObject>(AssetLoader.c_OverlayPath + popUpState.ToString()), Instance.m_Canvas.transform);

            // setup initalization depending on the popup state
            switch (popUpState) {
                case EPopUpState.ChestOpeningScreen:
                    EChestType chestType = (EChestType)args[0];
                    int chestIndex = args.Length > 1 ? (int)args[1] : -1;
                    obj.GetComponent<ChestOpeningScreen>().Initialize(chestType, chestIndex);
                    break;

                default:
                    obj.GetComponent<OverlayScreen>().Initialize();
                    break;
            }
        }

        #endregion


        #region Listeners

        void OnSignedIn()
        {
            m_SignedIn = true;
            PlayerData.PlayerName = AuthenticationService.Instance.PlayerName;
        }

        private void OnInitializationCompleted()
        {
            Debug.Log("Main.OnInitializationCompleted()");

            if (SceneLoader.Instance == null)
                Debug.Log("SceneLoader is null");

            SceneLoader.Instance.LoadScene("MainMenu");
        }

        private void OnStateChanged(EAppState state)
        {
            Debug.Log("New state : " + state.ToString());
        }

        #endregion
    }
}