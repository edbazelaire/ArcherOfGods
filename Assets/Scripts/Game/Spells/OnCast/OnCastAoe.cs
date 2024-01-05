using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Spells
{
    public class OnCastAoe : MonoBehaviour
    {
        #region Init & End

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        public void Initialize(float radius)
        {
            transform.localScale = new Vector3(radius, transform.localScale.y, transform.localScale.z);
        }

        #endregion
    }
}