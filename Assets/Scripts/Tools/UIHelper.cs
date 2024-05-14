using Enums;
using Game.Loaders;
using Managers;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        /// Get Width and Height of a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GetSize(GameObject gameObject, out float width, out float height)
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

            width = rectTransform.rect.width;
            height = rectTransform.rect.height;
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


        #region Spawning

        public static void SpawnCharacter(ECharacter character, GameObject parent, string layerName = "")
        {
            // clean container before spawning
            CleanContent(parent);

            // get selected character preview
            var characterData = CharacterLoader.GetCharacterData(character);
            var characterPreview = characterData.InstantiateCharacterPreview(parent);

            // display character preview
            var baseScale = characterPreview.transform.localScale;
            var parentRect = Finder.FindComponent<RectTransform>(parent);
            float scaleFactor = parentRect.rect.height / characterPreview.transform.localScale.y;
            characterPreview.transform.localScale = new Vector3(baseScale.x * scaleFactor, baseScale.y * scaleFactor, 1f);

            // adjust ordering of the character preview to be above canvas
            AdjustLayout(characterPreview, layerName);
        }

        #endregion


        #region Layout Managing

        public static Canvas GetFirstCanvas(Transform child)
        {
            // Traverse up the hierarchy until a Canvas component is found
            while (child != null)
            {
                Canvas canvas = child.GetComponent<Canvas>();
                if (canvas != null)
                {
                    return canvas; // Found a Canvas component
                }

                // Move up to the parent transform
                child = child.parent;
            }

            // No Canvas component found in the hierarchy
            ErrorHandler.Error("Unable to find canvas in " + child.name);
            return null; 
        }

        /// <summary>
        /// Adjust layout ordering to put gameObject above parent
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="parent"></param>
        public static void AdjustLayout(GameObject gameObject, string layerName = "")
        {
            Canvas canvas = GetFirstCanvas(gameObject.transform);

            // Get all SpriteRenderer components attached to this GameObject and its children
            SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>();

            // Set sorting layer and order in layer for each sprite renderer
            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                renderer.sortingLayerName   = layerName == "" ? canvas.sortingLayerName : layerName;
                renderer.sortingOrder       += canvas.sortingOrder;
            }
        }

        #endregion
    }
}