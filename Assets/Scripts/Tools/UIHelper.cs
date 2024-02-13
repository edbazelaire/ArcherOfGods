using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public static class UIHelper
    {
        public static void CleanContent(GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}