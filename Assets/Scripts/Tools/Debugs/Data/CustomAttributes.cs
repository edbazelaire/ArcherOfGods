using System;
using UnityEngine;

namespace Tools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class Command : PropertyAttribute
    {
        public KeyCode      KeyCode         { get; }
        public string       CommandName     { get; }

        public Command(KeyCode keyCode = KeyCode.None, string commandName = "")
        {
            KeyCode = keyCode;
            CommandName = commandName;
        }
    }
}