using Assets;
using System;
using System.Collections;

namespace Tools
{
    public static class CoroutineManager
    {
        public static void DelayMethod(Action method, int nFrames = 1)
        {
            Main.Instance.StartCoroutine(DelayMethodByFrames(method, nFrames));
        }

        static IEnumerator DelayMethodByFrames(Action method, int nFrames = 1)
        {
            while (--nFrames >= 0)
            {
                yield return null;
            }

            method?.Invoke();
        }
    }
}