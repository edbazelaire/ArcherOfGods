using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        #region Members

        const string c_LoadingBar = "LoadingBar";

        Image m_LoadingBar;

        #endregion


        // Use this for initialization
        void Awake()
        {
            m_LoadingBar = Finder.FindComponent<Image>(gameObject, c_LoadingBar);

            Debug.Log("Loading bar : " + m_LoadingBar);

            DontDestroyOnLoad(gameObject);
        }

        public void SetProgress(float progress)
        {
            m_LoadingBar.fillAmount = progress;
        }

        public void Display(bool display)
        {
            gameObject.SetActive(display);
        }
    }
}