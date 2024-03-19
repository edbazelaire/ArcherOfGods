using System.Collections.Generic;

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
        public static bool NotNull(object obj)
        {
            return obj != null;
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
        public static bool CheckSize<T>(T[] array, int size, bool atLeast = false)
        {
            if (atLeast)
                return array.Length >= size;
            else
                return array.Length == size;
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
        public static bool CheckEmpty<T>(T[] array)
        {
            return array.Length != 0;
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
        public static bool CheckEmpty<T>(List<T> array)
        {
            return CheckEmpty(array.ToArray());
        }
    }
}
