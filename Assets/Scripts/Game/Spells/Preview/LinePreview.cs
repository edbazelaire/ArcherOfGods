using Game.Character;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Spells
{
    public class LinePreview : SpellPreview
    {
        #region Members


        #endregion


        #region Inherited Manipulators

        public override void Initialize(Transform targettableArea, float distance)
        {
            base.Initialize(targettableArea, distance);

            var currentScale = m_Graphics.transform.localScale;
            currentScale.x = distance;

            // update size of line
            m_Graphics.transform.localScale = currentScale;
            // move line to be ont the eadge of the character
            transform.localPosition = new Vector3(transform.localPosition.x + distance / 2, transform.localPosition.y, transform.localPosition.z);
        }

        #endregion


        #region Private Manipulators


        #endregion
    }
}