using Assets;
using Enums;
using System.Collections;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class OverlayScreen : OvMonoBehavior
    {
        protected bool m_Initialized = false;

        protected EPopUpState m_Ref;
        protected virtual string PrefabPath { get; set; } = AssetLoader.c_OverlayPath;


        public OverlayScreen(EPopUpState reference)
        {
            m_Ref = reference;
        }

        public virtual void Initialize()
        {
            OnEnter();
            CoroutineManager.DelayMethod(OnPrefabLoaded);

            m_Initialized = true;
        }

        protected virtual void OnEnter()
        {

        }

        protected virtual void OnExit()
        {
            UnRegisterButtons();
            Destroy(gameObject);
        }

        #region Loading

        protected virtual void OnPrefabLoaded() 
        {
            RegisterButtons();
        }

        #endregion


        #region Updates

        protected virtual void Update()
        {
            if (!m_Initialized)
                return;

            CheckTouch();
        }

        void CheckTouch()
        {
            if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
            {
                GameObject clickedObject = EventSystem.current.currentSelectedGameObject;

                // dont do anything on touching button 
                if (clickedObject != null && clickedObject.GetComponent<Button>() != null)
                    return;

                // Invoke the event with the clicked GameObject as parameter
                OnTouch(clickedObject);
            }
        }

        #endregion


        #region Buttons

        protected virtual void RegisterButtons()
        {
            var buttons = Finder.FindComponents<Button>(gameObject, throwError: false);
            if (buttons == null)
                return;

            foreach (var button in buttons)
            {
                button.onClick.AddListener(() => OnUIButton(button.name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void UnRegisterButtons()
        {
            var buttons = Finder.FindComponents<Button>(gameObject, throwError: false);
            if (buttons == null)
                return;

            foreach (var button in buttons)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// When a button is clicked : call a listener method depending on the name 
        /// </summary>
        /// <param name="bname"></param>
        protected virtual void OnUIButton(string bname)
        {
            switch (bname)
            {
                default:
                    //ErrorHandler.Warning("Unregistered button : " + bname);
                    return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colliderName"></param>
        protected virtual void OnTouch(GameObject gameObject)
        {

        }

        #endregion


        
    }
}