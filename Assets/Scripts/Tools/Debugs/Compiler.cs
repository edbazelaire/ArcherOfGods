using MyBox;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;

namespace Tools
{
    static class Compiler
    {
        // ================================================================================
        // REGEXES UNITARY
        const string REGEX_CLASS        = @"(?:[A-Z]{1}[a-z]*)+";
        const string REGEX_METHOD       = @"\w+";


        #region Code Execution

        /// <summary>
        /// From a string input provided by the Console, check if this matches one of our REGEXES that will generate a code execution
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool ExecuteCode(string input, out object output)
        {
            output = null;

            // check that this input matching a variable assignation
            if (MatchVariableAssignement(input, out string varName, out string call))
            {
                if (ExecuteVariableAssignement(varName, call))
                    return true;

                Debug.Log("input : " + input + " | was regognized as variable assignment but was unable to be executed");
            }

            // check if matches the regex of a class method call
            if (MatchClassMethodCall(input, out string className, out string methodName, out string[] args))
            {
                // check that the execution went trought
                if (ExecuteClassMethodCall(className, methodName, args, out output))
                    return true;
            }

            // try to execute this input as a simple value call (raw int, float, string, ...)
            if (ExecuteDefaultValue(input, out output))
                return true;

            if (ExecuteVariable(input, out output))
                return true;

            return false;
        }

        /// <summary>
        /// Execute the call of a class method
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        static bool ExecuteClassMethodCall(string className, string methodName, string[] args, out object output)
        {
            output = null;

            // ==========================================================================================
            // [CLASS] : get class from alived classes 
            object myClass = Debugger.Instance.GetClass(className);
            if (myClass == null)
            {
                ErrorHandler.Error("class call pattern recognized but class " + className + " not found");
                return false;
            }

            // ==========================================================================================
            // [METHOD] : get method from the object 
            var method = myClass.GetType().GetMethod(methodName);
            if (method == null)
            {
                ErrorHandler.Error("method of class " + className + " not found");
                return false;
            }

            // ==========================================================================================
            // [METHOD ARGS] : get arguemnts of method and convert string args[] to their requrested types
            ParameterInfo[] parameters = method.GetParameters();
            if (args.Length > parameters.Length)
            {
                ErrorHandler.Error($"Method {methodName} expects {parameters.Length} at most : provided {args.Length}");
                return false;
            }

            // get parameters of the method
            object[] methodArgs = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                // convert string parameters to expected types
                if (i < args.Length)
                    methodArgs[i] = ConvertRawString(args[i], parameters[i].ParameterType);

                // if no args provided : use default value
                else
                    methodArgs[i] = parameters[i].DefaultValue;
            }

            // ==========================================================================================
            // [CALL] : call the method of the class with provided args
            output = method.Invoke(myClass, methodArgs);

            return true;
        }

        /// <summary>
        /// Execute code to create a new variable in the Debugger
        /// </summary>
        /// <param name="varName">  name of the variable to assign                                  </param>
        /// <param name="call">     string expression ton execute to get the value of the variable  </param>
        /// <returns></returns>
        static bool ExecuteVariableAssignement(string varName, string call)
        {
            if (! ExecuteCode(call, out object value))
                return false;

            Debugger.AddVariable(varName, value);
            return true;
        }

        /// <summary>
        /// Check if provided input is a known variable, execute the variable or return false otherwise
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        static bool ExecuteVariable(string input, out object output)
        {
            output = null;
            input = input.Trim();
            if (! Debugger.HasVariable(input)) 
                return false;

            output = Debugger.Variables[input];
            return true;
        }

        /// <summary>
        /// Read a string and return the converted default value if possible to interpret
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        static bool ExecuteDefaultValue(string input, out object output) 
        {
            output = null;
            Type castType = GetType(input);
            if (castType == null) 
                return false;

            output = ConvertRawString(input, castType);
            return true;
        }

        #endregion


        #region Matching Manipulators

        /// <summary>
        /// Check if provided string input is a call of a method from a class
        /// </summary>
        /// <param name="input">        input string provided by the console        </param>
        /// <param name="className">    [OUT] name of the class called              </param>
        /// <param name="methodName">   [OUT] name of the method called             </param>
        /// <param name="args">         [OUT] arguments called in the method        </param>
        /// <returns></returns>
        public static bool MatchClassMethodCall(string input, out string className, out string methodName, out string[] args)
        {
            className = "";
            methodName = "";
            args = new string[0];

            Match match = (new Regex(ClassMethodCallRegex())).Match(input);
            if (!match.Success)
                return false;

            className = match.Groups[1].Value;
            methodName = match.Groups[2].Value;

            // check if args content can be extracted
            if (!MatchArguments(match.Groups[3].Value, out args))
                return false;

            return true;
        }

        /// <summary>
        /// From a raw string arguments call, extract each arguments
        /// </summary>
        /// <param name="input"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool MatchArguments(string input, out string[] args)
        {
            args = new string[0];

            // no argument is ok
            if (input == "")
                return true;

            // Split the string using the regex pattern
            args = Regex.Split(input, SplitArgsRegex());
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool MatchVariableAssignement(string input, out string varName, out string call)
        {
            varName = "";
            call    = "";

            Match match = Regex.Match(input, VariableAllocationRegex());
            if (! match.Success) 
                return false;

            varName = match.Groups[1].Value;
            call    = match.Groups[2].Value;

            return true;
        }

        #endregion


        #region Type Conversion

        /// <summary>
        /// Convert string value into expected type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        static object ConvertRawString(string value, Type expectedType)
        {
            if (expectedType.IsEnum)
            {
                if (value.All(char.IsDigit))
                    return Enum.ToObject(expectedType, ConvertRawString(value, typeof(int)));

                if (Enum.TryParse(expectedType, value, out object outputValue))
                    return outputValue;

                ErrorHandler.Error("Unable to convert string " + value + " in Enum : " + expectedType);
                return null;
            }

            if (IsString(value) && expectedType == typeof(string))
            {
                return value.Substring(1, value.Length - 2);
            }

            return Convert.ChangeType(value, expectedType);
        }

        static Type GetType(string value) 
        { 
            if (IsInt(value))
                return typeof(int);

            if (IsString(value))
                return typeof(string);

            return null;
        }

        static bool IsInt(string value) 
        {
            return value.All(char.IsDigit);
        }

        static bool IsString(string value) 
        {
            value = value.Trim();
            return value.StartsWith("\"") && value.EndsWith("\"");
        }

        #endregion


        #region Regex Manipulators

        static string Capture(string pattern)
        {
            return @"(" + pattern + @")";
        }

        static string ClassMethodCallRegex()
        {
            return Capture(REGEX_CLASS) + @"\." + MethodCallRegex();
        }

        static string MethodCallRegex()
        {
            return Capture(REGEX_METHOD) + @"\("+ Capture(".*") + @"\)";
        }

        static string SplitArgsRegex()
        {
            return @"\s*,\s*(?=(?:[^()]*\([^()]*\))*[^()]*$)";
        }

        static string VariableAllocationRegex()
        {
            return @"^\s*(\w+)\s*=\s*(.*)$";
        }

        #endregion
    }
}