using UnityEngine;

namespace Assets.Scripts.Game.Spells.Other
{
    public class Rotate : MonoBehaviour
    {
        public float RotationPerSeconds = 1f;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(0, 0, transform.rotation.z - RotationPerSeconds * Time.deltaTime * 360);
        }
    }
}