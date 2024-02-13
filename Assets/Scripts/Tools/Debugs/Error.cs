using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public enum EError { Log, Warning, Error, FatalError, Count }

    public class Error : System.Exception
    {
        public Error(string message, EError type = EError.Error) : base(message)
        {
            switch (type)
            {
                case EError.Log:
                    Debug.Log(message);
                    break;
                case EError.Warning:
                    Debug.LogWarning(message);
                    break;
                case EError.Error:
                    Debug.LogError(message);
                    break;
                case EError.FatalError:
                    break;
                default:
                    break;
            }

            if (type == EError.FatalError)
                throw this;
        }
    }
}