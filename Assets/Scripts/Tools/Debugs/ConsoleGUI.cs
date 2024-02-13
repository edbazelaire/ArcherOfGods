using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Windows;

public class ConsoleToGUI : MonoBehaviour
{
    string myLog = "*begin log";
    string filename = "";
    bool doShow = true;
    int nLines = 18;
    void OnEnable() { Application.logMessageReceived += Log; DontDestroyOnLoad(this); }
    void OnDisable() { Application.logMessageReceived -= Log; }
    void Update() { if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) { doShow = !doShow; } }
    public void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        myLog += "\n" + logString;
        myLog = KeepLastLines(myLog, nLines);

        // for the file ...
        if (filename != "")
        {
            try
            {
                System.IO.File.AppendAllText(filename, logString + "\n");
            }
            catch { }
        }
    }
        

    void OnGUI()
    {
        if (!doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
           new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 10, 540, 370), myLog);
    }

    string KeepLastLines(string myLog, int nLines)
    {
        int nLinesToRemove = Regex.Matches(myLog, "\n").Count - nLines;
        if (nLinesToRemove > 0)
        {
            foreach (char c in myLog)
            {
                if (c == '\n')
                {
                    nLinesToRemove--;
                    myLog = myLog[c..];

                    if (nLinesToRemove == 0)
                    {
                        break;
                    }
                }
            }
        }

        return myLog;
    }
}