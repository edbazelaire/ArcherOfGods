using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class Movement : NetworkBehaviour
    {
        #region Members

        public float BASE_PLAYER_SPEED = 3f;

        Controller                  m_Controller;

        NetworkVariable<int>        m_MoveX                 = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        NetworkVariable<bool>       m_MovementCancelled     = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        float m_InitialSpeed;

        #endregion


        #region Inherited Manipulators

        void Awake()
        {
            m_Controller = GetComponent<Controller>();
        }   

        void Update()
        {
            if (IsOwner)
                CheckInputs();

            if (!IsServer)
                return;
            
            UpdateMovement();
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// Initialize player movement speed
        /// </summary>
        /// <param name="characterSpeed"></param>
        public void Initialize(float characterSpeed)
        {
            if (!IsServer)
                return;

            m_InitialSpeed = BASE_PLAYER_SPEED * characterSpeed;
        }

        #endregion


        #region Private Manipulators

        /// <summary>
        /// Apply speed on position
        /// </summary>
        void UpdateMovement()
        {
            if (m_MoveX.Value == 0 || ! CanMove)
                return;

            // depending on team, the camera is rotated implying that movement is inverted
            float teamFactor = m_Controller.Team == 0 ? 1f : -1f;
            transform.position += new Vector3(teamFactor * m_MoveX.Value * Speed * Time.deltaTime, 0f, 0f);

            // update rotation depending on movement (and team)
            if (teamFactor * m_MoveX.Value == 1)
                SetRotation(0f);
            else if (teamFactor * m_MoveX.Value == -1)
                SetRotation(180f);
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

            if (! CanMove || ! m_Controller.IsPlayer)
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
            }
            else if (Input.GetKey(KeyCode.D))
            {
                m_MoveX.Value = 1;
            } 
        }

        void ResetRotation()
        {
            if (!IsServer)
                return;

            SetRotation(m_Controller.Team == 0 ? 0f : -180f);
        }

        #endregion


        #region Public Manipulators

        public void CancelMovement(bool cancel)
        {
            if (cancel)
            {
                // reset rotation locally
                ResetRotation();

                // reset rotation on server side
                ResetRotationServerRPC();
            }

            if (cancel && ! IsMoving)
                return;

            m_MovementCancelled.Value = cancel;

            if (cancel)
            {
                m_MoveX.Value = 0;
            }
        }

        [ServerRpc]
        public void ResetRotationServerRPC()
        {
            if (!IsServer)
                return;

            ResetRotation();
        }

        #endregion


        #region Dependent Attributes

        public float Speed
        {
            get { return Math.Max(0, m_InitialSpeed * m_Controller.StateHandler.SpeedBonus.Value); }
        }

        public bool IsMoving
        {
            get { return m_MoveX.Value != 0; }
        }

        public bool CanMove
        {
            get
            {
                return
                    ! m_Controller.StateHandler.IsStunned 
                    && ! m_Controller.CounterHandler.HasCounter
                    && ! m_Controller.StateHandler.HasState(Enums.EStateEffect.Jump);
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
