using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// Handles the cleaning of text, formats, colors, ...
    /// </summary>
    public static class TextHandler
    {
        /// <summary> when put in a text, all lignes will with this tag will have enought spaces to match alignement </summary>
        public const string TAG_ALIGNMENT = "%%ALIGNMENT%%";

        public static string Clean(string text)
        {
            CleanAlignment(ref text);

            return text;
        }

        static void CleanAlignment(ref string text)
        {
            // check if has tag
            if (! HasTag(text, TAG_ALIGNMENT))
                return;

            // =================================================================================================
            // FIND ALIGNEMENT POSITIONS

            // split text line by line
            string[] lignes = text.Split("\n");

            // get max alignement pos of each ALIGNMENT sections (can be multiples in one line)
            List<int> alignementPos = new(0);
            foreach(string line in lignes)
            {
                // skip line if has no TAG
                if (!HasTag(line, TAG_ALIGNMENT))
                    continue;

                string[] lineSplits = line.Split(TAG_ALIGNMENT);
                for (int i=0; i < lineSplits.Length - 1; i++) 
                {
                    // save provided position of the alignement if is sup to last one (or if does not exists)
                    if (i < alignementPos.Count)
                        alignementPos[i] = Math.Max(lineSplits[i].Length, alignementPos[i]);
                    else
                        alignementPos.Add(lineSplits[i].Length);
                }
            }

            // =================================================================================================
            // APPLY ALIGNEMENT
            
            // reset text
            text = "";
            foreach (string line in lignes)
            {
                string lineCleaned = "";
                string[] lineSplits = line.Split(TAG_ALIGNMENT);
                for (int i = 0; i < lineSplits.Length; i++)
                {
                    // check if must add spaces (for the alignement) before adding the next line
                    string spaces = i > 0 ? string.Concat(Enumerable.Repeat(" ", 2 * (alignementPos[i-1] - lineSplits[i-1].Length))) : "";
                    lineCleaned += spaces + lineSplits[i];
                }

                text += (text != "" ? "\n" : "") + lineCleaned;
            }
        }

        static bool HasTag(string text, string tag) 
        { 
            return text.Contains(tag);
        }
    }
}