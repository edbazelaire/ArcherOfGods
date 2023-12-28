using UnityEngine;

namespace Game.Character
{
    public class Movement : MonoBehaviour
    {
        public float InitialSpeed;

        int m_MoveX = 0;
        float m_SpeedFactor = 1f;

        Controller m_Controller;

        // Start is called before the first frame update
        void Start()
        {
            m_Controller = GetComponent<Controller>();
        }

        // Update is called once per frame
        void Update()
        {
            SelectMovement();
            UpdateMovement();
        }

        #region Update Methods

        void SelectMovement()
        {
            if (m_Controller.IsPlayer)
            {
                CheckInputs();
                return;
            }

            MovementIA();
            return;
        }

        /// <summary>
        /// Apply speed on position
        /// </summary>
        void UpdateMovement()
        {
            transform.position += new Vector3(m_MoveX * Speed * Time.deltaTime, 0f, 0f);
        }

        #endregion


        #region Private Manipulators


        /// <summary>
        /// Check if movement inputs have beed pressed
        /// </summary>
        void CheckInputs()
        {
            m_MoveX = 0;

            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
                m_MoveX = -1;
            else if (Input.GetKey(KeyCode.D))
                m_MoveX = 1;
        }

        void MovementIA()
        {
            return;
        }

        #endregion


        #region Dependent Attributes

        public float Speed
        {
            get { return InitialSpeed * m_SpeedFactor; }
        }

        #endregion
    }
}
