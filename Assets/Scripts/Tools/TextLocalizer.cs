using System;

namespace Tools
{
    public static class TextLocalizer
    {
        const string TIMER_FORMAT_HMS = "{0:D2} : {1:D2} : {2:D2}";

        public static string LocalizeText(string text)
        {
            return text;
        }

        public static string GetAsCounter(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return string.Format(TIMER_FORMAT_HMS,
                time.Hours,
                time.Minutes,
                time.Seconds
            );
        }
    }
}