using Assets;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using Save.RSDs;
using System.Collections;

namespace Save
{
    public static class RSDManager
    {
        #region Members

        public static List<RSD> m_RSDList;

        public static List<RSD> RSDList => m_RSDList;

        #endregion


        #region Init & End

        public static void Intialize()
        {
            LoadSave();
        }

        #endregion


        #region Load & Save

        public static void LoadSave()
        {
            m_RSDList = new()
            {
                new TokensRSD(),
            };
        }

        #endregion


        #region Accessors

        public static T GetRSD<T>() where T : RSD, new()
        {
            foreach (RSD rsd in m_RSDList)
            {
                if (rsd.GetType() == typeof(T))
                    return (T)rsd;
            }

            ErrorHandler.Warning($"RSD {typeof(T)} not found in CloudSaveManager - creating new one");

            // Create an instance of the class represented by rsdType
            var instance = new T();
            m_RSDList.Add(instance);

            return instance;
        }

        /// <summary> check that all cloud data have been loaded </summary>
        public static bool LoadingCompleted
        {
            get
            {
                // check initialized
                if (m_RSDList == null || m_RSDList.Count == 0)
                    return false;

                // check all cloud data have beed loaded
                foreach (RSD rsd in m_RSDList)
                {
                    if (!rsd.LoadingCompleted)
                        return false;
                }

                return true;
            }
        }

        #endregion

    }
}