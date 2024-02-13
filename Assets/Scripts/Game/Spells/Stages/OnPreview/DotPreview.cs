using Game.Character;
using UnityEngine;

namespace Game.Spells
{
    public class DotPreview : GroundPreview
    {

        #region Inherited Manipulators

        public override void Initialize(Transform targettableArea, float distance, float radius = 0)
        {
            base.Initialize(targettableArea, distance);
            transform.localScale = new Vector3(1, 1, 0f);
        }
        
        #endregion
    }
}