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
    }
}
