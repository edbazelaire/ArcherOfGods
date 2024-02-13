using Game.Character;
using UnityEngine;

namespace Game.Spells
{
    public class GroundPreview : SpellPreview
    {

        #region Inherited Manipulators

        public override void Initialize(Transform targettableArea, float distance, float radius = 0)
        {
            base.Initialize(targettableArea, distance);
            transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 0f, 0.1f);

            if (radius <= 0)
                return;

            transform.localScale = new Vector3(radius, transform.localScale.y, transform.localScale.y);
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