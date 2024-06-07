using Game;
using System;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace AI
{
    public abstract class BehaviorTree : NetworkBehaviour
    {
        #region Members

        private Node m_Root = null;
        protected Controller m_Controller;
        protected bool m_IsActivated = false;

        protected float m_DecisionDeltaTime = 0.2f;
        protected float m_DecisionTimer = 0f;

        public bool IsActivated => m_IsActivated;

        #endregion


        #region Init & End

        public void Initialize()
        {
            m_Controller = Finder.FindComponent<Controller>(gameObject);
            m_Root = SetupTree();

            m_IsActivated = false;
        }

        public void Activate(bool activated = true)
        {
            m_IsActivated = activated;
        }

        #endregion

        private void Update()
        {
            if (!m_IsActivated || !IsServer)
                return;

            if (m_DecisionTimer > 0f)
            {
                m_DecisionTimer -= Time.deltaTime;
                return;
            }

            if (GameManager.IsGameOver)
            {
                Activate(false);
                return;
            }

            m_DecisionTimer = m_DecisionDeltaTime;

            if (m_Root != null)
                m_Root.Evaluate();
        }

        protected abstract Node SetupTree();
    }

}
