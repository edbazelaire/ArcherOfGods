using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Assets.Scripts.Tools
{
    public class TimeErrorWrapper : MonoBehaviour
    {
        #region Members

        static TimeErrorWrapper s_Instance;

        Dictionary<string, Coroutine> m_Coroutines = new();

        #endregion


        #region Init & End

        #endregion


        #region Coroutine Management

        public IEnumerator WrapCoroutine(string id, float timer, Action onTimerEnd)
        {
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            onTimerEnd?.Invoke();

            // remove from list of coroutines
            if (!m_Coroutines.ContainsKey(id))
            {
                ErrorHandler.Error("Unable to find coroutine with id : " + id);
                yield break;
            }

            m_Coroutines.Remove(id);
        }

        public void New(string id, float timer, Action onTimerEnd)
        {
            if (timer < 0)
            {
                ErrorHandler.Error("Time wrapper provided with negative timer ("+timer+"): cancelling");
                return;
            }

            if (id == "")
            {
                ErrorHandler.Error("Time wrapper provided with empty id : cancelling");
                return;
            }
            
            // if coroutine already exisiting : cancel before creating new one
            if (m_Coroutines.ContainsKey(id) && m_Coroutines[id] != null)
            {
                Cancel(id);
            }

            m_Coroutines[id] = StartCoroutine(WrapCoroutine(id, timer, onTimerEnd));
        }

        public void Cancel(string id)
        {
            if (! m_Coroutines.ContainsKey(id))
            {
                return;
            }

            StopCoroutine(m_Coroutines[id]);
            m_Coroutines.Remove(id);
        }

        #endregion


        #region Dependent Members

        public static TimeErrorWrapper Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = GameObject.Instantiate(AssetLoader.LoadManager<TimeErrorWrapper>());
                    DontDestroyOnLoad(s_Instance);
                }

                return s_Instance;
            }
        }  

        #endregion
    }
}