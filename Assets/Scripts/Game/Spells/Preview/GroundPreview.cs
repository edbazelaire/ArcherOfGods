using Game.Character;
using UnityEngine;

namespace Game.Spells
{
    public class GroundPreview : SpellPreview
    {

        #region Inherited Manipulators

        public override void Initialize(Transform targettableArea, float distance)
        {
            base.Initialize(targettableArea, distance);
            transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 0f, 0.1f);
        }

        protected override void Update()
        {
            base.Update();

            var MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            transform.position = new Vector3(MousePosition.x, 0f, 0.1f);
        }

        #endregion
    }
}