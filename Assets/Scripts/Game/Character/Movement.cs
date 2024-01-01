using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class Movement : NetworkBehaviour
    {
        #region Members

        public float InitialSpeed;

        float m_SpeedFactor = 1f;

        NetworkVariable<int> m_MoveX = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> m_MovementCancelled = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        #endregion


        #region Inherited Manipulators

        // Update is called once per frame
        void Update()
        {
            if (!IsOwner)
                return;

            CheckInputs();
            UpdateMovement();
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// Apply speed on position
        /// </summary>
        void UpdateMovement()
        {
            transform.position += new Vector3(m_MoveX.Value * Speed * Time.deltaTime, 0f, 0f);
            //UpdateMovementAnimationServerRpc();
        }

        void SetRotation(float y)
        {
            transform.rotation = Quaternion.Euler(0f, y, 0f);
        }

        /// <summary>
        /// Check if movement inputs have beed pressed
        /// </summary>
        void CheckInputs()
        {
            m_MoveX.Value = 0;

            // if movement has been cancelled, wait for all inputs to be released
            if (m_MovementCancelled.Value)
            {
                if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
                    m_MovementCancelled.Value = false;
                return;
            }

            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
            {
                m_MoveX.Value = -1;
                SetRotation(180f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                m_MoveX.Value = 1;
                SetRotation(0f);
            } 
        }

        #endregion


        #region Public Manipulators

        public void CancelMovement(bool cancel)
        {
            if (cancel && ! IsMoving)
                return;

            m_MovementCancelled.Value = cancel;

            if (cancel)
            {
                m_MoveX.Value = 0;
                //UpdateMovementAnimationServerRpc();
            }
        }

        #endregion


        #region Dependent Attributes

        public float Speed
        {
            get { return InitialSpeed * m_SpeedFactor; }
        }

        public bool IsMoving
        {
            get { return m_MoveX.Value != 0; }
        }

        #endregion

        public void DebugMessage()
        {
            Debug.Log("MoveX : " + m_MoveX.Value);
            Debug.Log("IsMoving : " + IsMoving);
            Debug.Log("Rotation : " + transform.rotation);
        }
    }
}
