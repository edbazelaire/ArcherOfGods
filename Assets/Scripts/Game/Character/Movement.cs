using Data.GameManagement;
using Enums;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class Movement : NetworkBehaviour
    {
        #region Members

        Controller                  m_Controller;

        NetworkVariable<int>        m_MoveX                 = new(0);
        NetworkVariable<bool>       m_MovementCancelled     = new(false);
        NetworkVariable<bool>       m_MovementBlocked       = new(false);

        float m_InitialSpeed;

        #endregion


        #region Init & End

        void Awake()
        {
            m_Controller = GetComponent<Controller>();
        }

        /// <summary>
        /// Initialize player movement speed
        /// </summary>
        /// <param name="characterSpeed"></param>
        public void Initialize(float characterSpeed)
        {
            if (!IsServer)
                return;

            m_InitialSpeed = characterSpeed;
        }

        void Update()
        {
            CheckInputs();

            if (!IsServer)
                return;
            
            UpdateMovement();
        }

        #endregion


        #region ServerRPC Methods

        [ServerRpc]
        public void SetMovementServerRPC(int moveX)
        {
            SetMovement(moveX);
        }

        public void SetMovement(int moveX)
        {
            if (!IsServer)
                return;

            if (!CanMove)
                moveX = 0;

            m_MoveX.Value = moveX;
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
            if (!IsOwner)
                return;

            if (! CanMove || ! m_Controller.IsPlayer)
                return;

            // if movement has been cancelled, wait for all inputs to be released
            if (m_MovementCancelled.Value)
            {
                if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
                    m_MovementCancelled.Value = false;
                return;
            }

            int moveX = 0;

            if (Input.GetKey(KeyCode.Q) || GameUIManager.LeftMovementButtonPressed)
            {
                moveX = -1;
            }
            else if (Input.GetKey(KeyCode.D) || GameUIManager.RightMovementButtonPressed)
            {
                moveX = 1;
            }

            SetMovementServerRPC(moveX);
        }

        void ResetRotation()
        {
            if (!IsServer)
                return;

            SetRotation(m_Controller.Team == 0 ? 0f : -180f);
        }

        #endregion


        #region Public Manipulators

        /// <summary>
        /// Add a little bit of movement and a reset rotation on server side to be sure that the clients synchronized properly
        /// </summary>
        public void Shake()
        {
            if (!IsServer)
                return;

            transform.position += new Vector3(0.01f, 0, 0);
            transform.rotation = Quaternion.Euler(0f, 0f, 0.01f);

            ResetRotation();
        }

        public void CancelMovement(bool cancel)
        {
            if (!IsServer)
                return;

            if (cancel)
                ResetRotation();

            if (cancel && ! IsMoving)
                return;

            m_MovementCancelled.Value = cancel;
            m_MoveX.Value = 0;
        }

        public void ForceBlockMovement(bool block)
        {
            if (! IsServer)
                return;

            CancelMovement(block);
            m_MovementBlocked.Value = block;
        }

        #endregion


        #region Dependent Attributes

        public float Speed
        {
            get { return Math.Max(0, Settings.CharacterSpeedFactor * m_InitialSpeed * m_Controller.StateHandler.SpeedBonus.Value); }
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
                    && ! m_MovementBlocked.Value
                    && ! m_MovementCancelled.Value
                    && ! m_Controller.StateHandler.HasState(EStateEffect.Frozen) 
                    && ! m_Controller.StateHandler.HasState(EStateEffect.Scorched) 
                    && ! m_Controller.CounterHandler.IsBlockingMovement.Value
                    && ! m_Controller.StateHandler.HasState(EStateEffect.Jump);
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
