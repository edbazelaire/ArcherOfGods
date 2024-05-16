using Assets.Scripts.Network;
using Enums;
using Game.Loaders;
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
using System.Linq;
using Data.GameManagement;
using System.Collections.Generic;
using Scripts.Menu.PopUps;
using Unity.Services.Core.Environments;
using Unity.Services.Analytics;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets
{
    public class Main : MonoBehaviour
    {
        #region Members

        static Main s_Instance;

        // ==========================================================================================================
        // SERIALIZED MEMBERS
        [SerializeField] Canvas m_Canvas;
        [SerializeField] CloudSaveManager m_CloudSaveManager;
        [SerializeField] float m_CharacterSizeFactor    = 1.0f;
        [SerializeField] float m_SpellSizeFactor        = 0.3f;    
        [SerializeField] List<ELogTag> m_LogTags;

        // ==========================================================================================================
        // EVENTS
        public static event Action<EAppState>   StateChangedEvent;
        public static event Action              InitializationCompletedEvent;
        public static event Action              ApplicationQuitEvent;

        // ==========================================================================================================
        // PRIVATE MEMBERS
        EAppState                               m_State                 = EAppState.Release;
        bool                                    m_SignedIn              = false;
        /// <summary> Events to store until reaching a specific AppState </summary>
        Dictionary<EAppState, List<Action>>     m_StoredEvents          = new();

        // ==========================================================================================================
        // PUBLIC DEPENDENT STATIC MEMBERS
        public static Main Instance                             => s_Instance;
        public static CloudSaveManager CloudSaveManager         => Instance.m_CloudSaveManager;
        public static EAppState State                           => Instance.m_State;
        public static Canvas Canvas                             => Instance.m_Canvas;
        public static float CharacterSizeFactor                 => Instance.m_CharacterSizeFactor;
        public static float SpellSizeFactor                     => Instance.m_SpellSizeFactor;
        public static List<ELogTag> LogTags                     => Instance.m_LogTags;  

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
                PlayerPrefsHandler.Initialize();
                AchievementLoader.Initialize();
                ItemLoader.Initialize();

                // register to state changes 
                StateChangedEvent += OnStateChanged;

                // register on initliazitation completed check by the Coroutine
                InitializationCompletedEvent += OnInitializationCompleted;

                StartCoroutine(CheckInitialization());

                // initialize UnituServices
                var options = new InitializationOptions();
                options.SetEnvironmentName("dev");
                await UnityServices.InitializeAsync(options);

                // listen to Auth Service and try to signe in anonymously
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                AuthenticationService.Instance.SignedIn += m_CloudSaveManager.LoadSave;
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            }
            catch (Exception ex)
            {
                ErrorHandler.Error(ex.Message);
            }

#if UNITY_EDITOR
            ErrorHandler.Log("UNITY EDITOR MODE", ELogTag.System);
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
                CharacterLoader.Instance            == null
                || SpellLoader.Instance             == null
                || SceneLoader.Instance             == null
                || ItemLoader.ChestRewardData       == null
                || AchievementLoader.Achievements   == null
                || RelayHandler.Instance            == null
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

        public static void AddStoredEvent(EAppState state, Action action)
        {
            // state is current state : fire event right away
            if (Instance.m_State == state)
            {
                action?.Invoke();
                return;
            }

            // init if doesnt exists
            if (!Instance.m_StoredEvents.ContainsKey(state))
                Instance.m_StoredEvents.Add(state, new List<Action>());

            // store the event
            Instance.m_StoredEvents[state].Add(action);
        }

        #endregion


        #region PopUp & Screens

        public static void SetPopUp(EPopUpState popUpState, params object[] args)
        {
            var popUpPath = "";
            if (popUpState.ToString().EndsWith("Screen"))
            {
                popUpPath = AssetLoader.c_OverlayPath;
            } else if (popUpState.ToString().EndsWith("PopUp"))
            {
                popUpPath = AssetLoader.c_PopUpsPath;
            } else
            {
                ErrorHandler.Error("Unknown PopUp type " + popUpState.ToString() + " : unable to find adequate path");
            }

            // instantiate object in the canvas
            var obj = Instantiate(AssetLoader.Load<GameObject>(popUpPath + popUpState.ToString()));
            if (obj == null)
            {
                ErrorHandler.Error("Unable to find popup : " + popUpState.ToString());
                return;
            }

            // setup initalization depending on the popup state
            switch (popUpState) {

                // MESSAGE POP UPS -------------------------------------------------------
                case EPopUpState.MessagePopUp:
                    obj.GetComponent<MessagePopUp>().Initialize((string)args[0], args.Count() > 1 ? (string)args[1] : "");
                    break;

                case EPopUpState.ConfirmBuyPopUp:
                    obj.GetComponent<ConfirmBuyPopUp>().Initialize((SPriceData)args[0], (SRewardsData)args[1]);
                    break;

                case EPopUpState.ConfirmBuyItemPopUp:
                    obj.GetComponent<ConfirmBuyItemPopUp>().Initialize((SPriceData)args[0], (Enum)args[1], (int)args[2]);
                    break;

                case EPopUpState.ConfirmBuyBundlePopUp:
                    obj.GetComponent<ConfirmBuyBundlePopUp>().Initialize((SPriceData)args[0], (SRewardsData)args[1]);
                    break;


                // SCREENS -------------------------------------------------------
                case EPopUpState.RewardsScreen:
                    obj.GetComponent<RewardsScreen>().Initialize((SRewardsData)args[0], (string)args[1]);
                    break;

                case EPopUpState.AchievementRewardScreen:
                    obj.GetComponent<AchievementRewardScreen>().Initialize((List<SAchievementRewardData>)args[0]);
                    break;

                case EPopUpState.ArenaPathScreen:
                    obj.GetComponent<ArenaPathScreen>().Initialize((EArenaType)args[0]);
                    break;

                case EPopUpState.LevelUpScreen:
                    obj.GetComponent<LevelUpScreen>().Initialize((ECharacter)args[0]);
                    break;

                // INFO POP UPS -------------------------------------------------------
                case EPopUpState.SpellInfoPopUp:
                    bool infoOnly = args.Length > 2 && (bool)args[2];
                    obj.GetComponent<SpellInfoPopUp>().Initialize((ESpell)args[0], (int)args[1], infoOnly);
                    break;

                case EPopUpState.CharacterInfoPopUp:
                    obj.GetComponent<CharacterInfoPopUp>().Initialize((ECharacter)args[0], (int)args[1]);
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

        public static void DisplayRewards(SRewardsData rewardsData, ERewardContext context)
        {
            Main.SetPopUp(EPopUpState.RewardsScreen, rewardsData, context.ToString());
        }
        

        public static void DisplayAchievementRewards(List<SAchievementRewardData> rewardsData)
        {
            Main.SetPopUp(EPopUpState.AchievementRewardScreen, rewardsData);
        }

        public static void ConfirmBuyRewards(SPriceData priceData, SRewardsData rewardsData)
        {
            if (rewardsData.Rewards.Count == 0)
            {
                ErrorHandler.Error("Call reward popup with no rewards in list");
                return;
            }

            if (rewardsData.Rewards.Count > 1)
            {
                Main.SetPopUp(EPopUpState.ConfirmBuyBundlePopUp, priceData, rewardsData);
                return;
            }

            if (rewardsData.Rewards.Count == 1 && Enum.TryParse(rewardsData.Rewards[0].RewardType, rewardsData.Rewards[0].RewardName, out object item) )
            {
                Main.SetPopUp(EPopUpState.ConfirmBuyItemPopUp, priceData, item, rewardsData.Rewards[0].Qty);
                return;
            }

            ErrorHandler.Warning("Unable to find any specific ConfirmBuyPopUp for provided RewardsData : using default");
            Main.SetPopUp(EPopUpState.ConfirmBuyPopUp, priceData, rewardsData);
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
            ErrorHandler.Log("SIGNED ID", ELogTag.System);
            m_SignedIn = true;

            // setup analytics
            MAnalytics.Initialize();
        }

        private void OnInitializationCompleted()
        {
            if (SceneLoader.Instance == null)
                ErrorHandler.Log("SceneLoader is null", ELogTag.System);

            ErrorHandler.Log("Initialization of the data completed : loading MainMenu", ELogTag.System);
            SceneLoader.Instance.LoadScene("MainMenu");
        }

        private void OnStateChanged(EAppState state)
        {
            ErrorHandler.Log("New state : " + state.ToString(), ELogTag.System);

            if (!m_StoredEvents.ContainsKey(state))
                return;
            
            foreach (Action action in m_StoredEvents[state])
            {
                action?.Invoke();
            }

            m_StoredEvents[state] = new List<Action>();
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