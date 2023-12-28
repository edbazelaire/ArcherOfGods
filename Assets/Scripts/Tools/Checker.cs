using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class Checker
    {
        /// <summary>
        /// Check if the object is null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool NotNull(object obj, string message = "")
        {
            if (obj == null)
            {
                ErrorHandler.NullObject(message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the array has the right size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        /// <param name="atLeast"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CheckSize<T>(T[] array, int size, bool atLeast = false, string message = "")
        {
            if (atLeast)
            {
                if (array.Length < size)
                {
                    ErrorHandler.ArraySize($"size of array ({array.Length}) is supposed to be at least {size} " + message);
                    return false;
                }
            }
            else
            {
                if (array.Length != size)
                {
                    ErrorHandler.ArraySize($"size of array ({array.Length}) is supposed to be {size} " + message);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the array has the right size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        /// <param name="atLeast"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CheckEmpty<T>(T[] array, string message = "")
        {
           
            if (array.Length == 0)
            {
                ErrorHandler.ArraySize($"array is empty " + message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the array has the right size
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="size"></param>
        /// <param name="atLeast"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CheckEmpty<T>(List<T> array, string message = "")
        {
            return CheckEmpty(array.ToArray(), message);
        }
    }
}
