using System.Collections;
using UnityEngine;

namespace Tools.Animations
{
    public class RotateAnimation : OvAnimation
    {
        #region Members

        [SerializeField] Vector3 m_Rotation;

        #endregion


        #region Init & End

        public void Initialize(string id = "", float duration = 1f, Vector3 rotation = default)
        {
            base.Initialize(id, duration);

            m_Rotation = rotation;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            transform.rotation = Quaternion.Euler(m_Rotation);
        }

        #endregion


        #region Animation

        protected override IEnumerator AnimationFrame()
        {
            float progress = GetProgress();

            // interpolate position
            transform.rotation = Quaternion.Euler(
                m_Rotation.x * progress, 
                m_Rotation.y * progress, 
                m_Rotation.z * progress
            );

            m_Timer += Time.deltaTime;
            yield return null;
        }


        #endregion
    }
}