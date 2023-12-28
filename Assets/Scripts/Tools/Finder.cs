using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Tools
{
    public static class Finder
    {
        public static GameObject Find(GameObject parent,  string name, bool throwError = true)
        {
            Transform child = parent.transform.Find(name);
            if (throwError && !Checker.NotNull(child))
                return null;

            return child.gameObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="throwError"></param>
        /// <returns></returns>
        public static List<GameObject> Finds(GameObject parent, string name, bool throwError = true)
        {
            // find all child with that name in parent
            List<GameObject> list = new List<GameObject>();
            Transform[] children = parent.transform.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (name.EndsWith("_"))
                {
                    if (child.name.StartsWith(name))
                        list.Add(child.gameObject);
                }
                else if (child.name == name)
                    list.Add(child.gameObject);
            }

            if (throwError && !Checker.CheckEmpty(list))
                return list;
            
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="throwError"></param>
        /// <returns></returns>
        public static T FindComponent<T>(GameObject parent, bool throwError = true)
        {
            var component = parent.transform.GetComponentInChildren<T>();
            
            if (throwError && !Checker.NotNull(component))
                return default(T);
            
            return component;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="throwError"></param>
        /// <returns></returns>
        public static List<T> FindComponents<T>(GameObject parent, bool throwError = true)
        {
            var components = parent.transform.GetComponentsInChildren<T>();

            if (throwError && !Checker.CheckSize<T>(components, 1, atLeast: true))
                return new List<T>();
            
            return new List<T>(components);
        }


        
    }
}