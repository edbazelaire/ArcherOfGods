using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class Movement : NetworkBehaviour
    {
        #region Members

        public float InitialSpeed;

        Controller              m_Controller;

        NetworkVariable<int>    m_MoveX                 = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        NetworkVariable<bool>   m_MovementCancelled     = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        #endregion


        #region Inherited Manipulators

        void Awake()
        {
            m_Controller = GetComponent<Controller>();
        }   

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

            if (! CanMove)
                return;

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
            }
        }

        #endregion


        #region Dependent Attributes

        public float Speed
        {
            get { return Math.Max(0, InitialSpeed + m_Controller.StateHandler.SpeedBonus); }
        }

        public bool IsMoving
        {
            get { return m_MoveX.Value != 0; }
        }

        public bool CanMove
        {
            get
            {
                return ! m_Controller.StateHandler.IsStunned && ! m_Controller.CounterHandler.HasCounter;
            }
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
