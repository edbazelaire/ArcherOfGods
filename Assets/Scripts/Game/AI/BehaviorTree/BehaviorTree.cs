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

            if (GameManager.IsGameOver)
            {
                Activate(false);
                return;
            }

            if (m_Root != null)
                m_Root.Evaluate();
        }

        protected abstract Node SetupTree();
    }

}
