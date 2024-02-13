using System.Threading;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class OnCastAoe : MonoBehaviour
    {
        #region Members

        const string c_BaseArea         = "BaseArea";
        const string c_GrowingArea      = "GrowingArea";

        GameObject  m_GrowingArea;
        Vector3     m_GrowingAreaBaseScale;

        /// <summary> duration of the aoe to proc </summary>
        float m_Duration;
        /// <summary> curent value of the timer </summary>
        float m_Timer;

        #endregion


        #region Init & End

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        public virtual void Initialize(Vector3 target, float radius, float duration)
        {
            m_GrowingArea = Finder.Find(gameObject, c_GrowingArea);

            // init position
            transform.position = new Vector3(target.x, 0f, 0f);

            // init scale
            transform.localScale                = new Vector3(radius, radius, transform.localScale.z);
            m_GrowingAreaBaseScale              = m_GrowingArea.transform.localScale;
            m_GrowingArea.transform.localScale  = Vector3.zero;

            // init timer
            m_Duration = duration;
            m_Timer = duration;
        }

        public virtual void End()
        {
            Destroy(gameObject);
        }

        #endregion


        #region Update Manipulators

        public virtual void Update()
        {
            m_Timer -= Time.deltaTime;
            if (m_Timer < 0)
            {
                End();
                return;
            }

            m_GrowingArea.transform.localScale = m_GrowingAreaBaseScale * (1 - (m_Timer / m_Duration));
        }

        #endregion
    }
}