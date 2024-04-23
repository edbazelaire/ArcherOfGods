using System.Collections;
using UnityEngine;

namespace Tools.Animations
{
    public class MoveAnimation : OvAnimation
    {
        #region Members

        [SerializeField] Vector3 m_StartPos;
        [SerializeField] Vector3 m_EndPos;

        #endregion


        #region Init & End

        public void Initialize(string id = "", float duration = 1f, Vector3? startPos = null, Vector3? endPos = null)
        {
            if (duration <= 0f)
            {
                ErrorHandler.Error("MoveAnimation can not be set as Inifinite, duration must be > 0 : " + duration);
                return;
            }

            base.Initialize(id, duration);

            m_StartPos      = startPos.HasValue ? startPos.Value : transform.position;
            m_EndPos        = endPos.HasValue ? endPos.Value : transform.position;

            // Set initial values immediately on initialization
            transform.position = m_StartPos;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            transform.position = m_EndPos;
        }

        #endregion


        #region Animation

        protected override IEnumerator AnimationFrame()
        {
            float progress = GetProgress();

            // interpolate position
            transform.position = new Vector3(Mathf.Lerp(m_StartPos.x, m_EndPos.x, progress), Mathf.Lerp(m_StartPos.y, m_EndPos.y, progress), 0f) ;

            m_Timer += Time.deltaTime;
            yield return null;
        }


        #endregion

    }
}