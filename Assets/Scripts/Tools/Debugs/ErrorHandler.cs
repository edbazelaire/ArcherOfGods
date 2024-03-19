using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Tools
{
    public static class ErrorHandler
    {
        #region Members

        public static List<Error> Errors = new List<Error>();

        #endregion


        #region Basic Errors

        public static void Log(string message, int frame = 0)
        {
            AddError(message, EError.Log, frame + 1);
        }

        public static void Warning(string message, int frame = 0)
        {
            AddError(message, EError.Warning, frame + 1);
        }

        public static void Error(string message, int frame = 0)
        {
            AddError(message, EError.Error, frame + 1);
        }

        public static void FatalError(string message, int frame=0)
        {
            AddError(message, EError.FatalError, frame + 1);
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
        static void AddError(string message, EError type = EError.Error, int frame = 0)
        {
            Errors.Add(new Error(message, type, frame+1));
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