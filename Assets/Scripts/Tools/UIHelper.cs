using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public static class UIHelper
    {
        /// <summary>
        /// Remove all childs of a container
        /// </summary>
        /// <param name="gameObject"></param>
        public static void CleanContent(GameObject gameObject)
        {
            if (gameObject == null)
            {
                ErrorHandler.Warning("Provided game object is null");
                return;
            }    

            foreach (Transform child in gameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Set a game objects anchors and sizeDelta to 100% match the size of the parent
        /// </summary>
        /// <param name="gameObject"></param>
        public static void SetFullSize(GameObject gameObject)
        {
            // get the RectTransform component of the GameObject
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

            // set the anchors to be at full length
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            // remove delta size
            rectTransform.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// Check if mouse is over position of a gameobject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static bool IsMouseIn(GameObject gameObject)
        {
            // Convert mouse position to local position of the scroller container
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                ErrorHandler.Error("Unable to define if mouse is in game object " +  gameObject.name + " because game object has no RectTransform value");
                return false;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out Vector2 localMousePos);

            // check if the local mouse position is within the bounds of the game object
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main);
        }
    }
}