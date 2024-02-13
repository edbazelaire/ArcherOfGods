using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class ErrorHandler
    {
        #region Members

        public static List<Error> Errors = new List<Error>();

        #endregion


        #region Basic Errors

        public static void Log(string message)
        {
            AddError(message, EError.Log);
        }

        public static void Warning(string message)
        {
            AddError(message, EError.Warning);
        }

        public static void Error(string message)
        {
            AddError(message, EError.Error);
        }

        public static void FatalError(string message)
        {
            AddError(message, EError.FatalError);
        }

        #endregion


        #region Specific Errors

        /// <summary>
        /// Add an error of type "Null Object" to the stack
        /// </summary>
        /// <param name="message"></param>
        public static void NullObject(string message = "")
        {
            AddError("Null object" + (message != "" ? " : " + message : ""), EError.Error);
        }

        /// <summary>
        /// Add an error of type "Array Size" to the stack
        /// </summary>
        /// <param name="message"></param>
        public static void ArraySize(string message)
        {
            AddError("Array size error" + (message != "" ? " : " + message : ""), EError.Error);
        }

        #endregion


        #region Error Stack Management

        /// <summary>
        /// Add an error to the stack
        /// </summary>
        /// <param name="error"></param>
        static void AddError(string message, EError type = EError.Error)
        {
            Errors.Add(new Error(message, type));
        }

        /// <summary>
        /// Reset all errors
        /// </summary>
        static void Reset()
        {
            Errors = new List<Error>();
        }

        #endregion
    }
}