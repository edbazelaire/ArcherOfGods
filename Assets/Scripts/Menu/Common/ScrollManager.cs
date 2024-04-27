using Menu.MainMenu;
using Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Assets.Scripts.Menu.Common
{
    public class ScrollManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        MainMenuManager m_MainMenuManager;
        ScrollRect      m_Scroller;

        private bool isHorizontalScrolling = false;

        void Start()
        {
            m_MainMenuManager = Finder.FindComponent<MainMenuManager>("MainMenuManager");
            m_Scroller = Finder.FindComponent<ScrollRect>(gameObject);

            // Initially, assume vertical scrolling is allowed
            m_Scroller.enabled = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_MainMenuManager == null || ! m_MainMenuManager.isActiveAndEnabled)
            {
                ErrorHandler.Warning("Calling method while MainMenuManager not accessible");
                return;
            }

            // Determine if the drag is more horizontal than vertical
            if (Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y))
            {
                isHorizontalScrolling = true;
                m_Scroller.enabled = false;   // Disable vertical scrolling
            }
            else
            {
                isHorizontalScrolling = false;
                m_Scroller.enabled = true;    // Enable vertical scrolling
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isHorizontalScrolling)
                m_MainMenuManager.OnDrag(eventData);
        }


        public void OnEndDrag(PointerEventData eventData)
        {
            // Re-enable vertical scrolling when horizontal drag ends
            m_Scroller.enabled = true;
        }
    }
}