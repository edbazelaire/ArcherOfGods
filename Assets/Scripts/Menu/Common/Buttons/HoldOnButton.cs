using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menu.Common.Buttons
{
    public class HoldOnButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Action<bool> PressedEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            PressedEvent?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PressedEvent?.Invoke(false);
        }
    }
}