using Enums;
using System;
using UnityEngine;

namespace Tools.Debugs
{
    public struct SCommand
    {
        public Action callback;
        public string name;
        public KeyCode keyCode;
        public string description;

        public SCommand(Action callback, string name = "", KeyCode keyCode = KeyCode.None, string description = "")
        {
            if (name == "" && keyCode == KeyCode.None)
                ErrorHandler.FatalError("registered callback with no name or keycode");

            this.callback = callback;
            this.name = name;
            this.keyCode = keyCode;
            this.description = description;
        }

        public void Display()
        {
            Debug.Log($"    + [{(keyCode != KeyCode.None ? keyCode.ToString() : " ")}] - {name} {(description != "" ? ": \n" + description : "")}");
        }
    }

    public struct SClass
    {
        public object Instance;
    }

    public struct SLog
    {
        public string Log;
        public string StackTrace;
        public LogType LogType;
        public ELogTag LogTag;

        public SLog(string log, string stackTrace, LogType logType, ELogTag logTag = ELogTag.None)
        {
            Log = log;
            StackTrace = stackTrace;
            LogType = logType;
            LogTag = logTag;
        }
    }
}